﻿using System.Text;


namespace ParserConsole;

internal class Program
{
    private static IHost? _host;
    
    private static TelegramBotClient? _botClient;
    private static readonly ConcurrentDictionary<long, CancellationTokenSource> _ctsDictionaryForRunningTask = new();
    private static List<(Func<string, bool> Predicate, Func<Message, Task> Handler)> _messageHandlers;

    private static void Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        _host = CreateHostBuilder().Build();

        var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ?? throw new InvalidOperationException("Токен телаграмм бота не задан.");

        try
        {
            _botClient = new TelegramBotClient(token);
        }
        catch (Exception ex)
        {
            
            Console.WriteLine("Telegram_Token: " + token);
            Console.WriteLine(ex.Message);
            throw;
        }
        
        _messageHandlers = new List<(Func<string, bool>, Func<Message, Task>)>
        {
            (data => data.StartsWith("/start"), HandleStartCommand),
            // (data => data.StartsWith("/allow"), HandleAllowTaggingCommand),
            // (data => data.StartsWith("/forbid"), HandleForbidTaggingCommand),
            // (data => data.StartsWith("/info"), HandleInfoPageCommand),
            // (data => data.StartsWith("/all"), HandleTagAllAsync),
        };

        _botClient.OnMessage += BotOnMessageReceived;

        // TODO жесткий костыль. При рефакторинге - убрать и сделать нормально. А пока без этого - программа завершается.
        while (true)
        {
            
        }
    }

    private static async Task HandleStartCommand(Message message)
    {
        await TryExecute(async () =>
        {
            await _botClient!.SendMessage(message.Chat.Id,
                """
                Чем я могу вам помочь?) 
                Я могу вам прислать текущую афишу БашОперы или подписаться на обновления афишы.

                /playbill - получить текущую афишу.
                /subscribe - подписаться на обновления афишы.

                Полный список команд можно получить через команду /help
                """);
        }, message.Chat.Id);
    }

    private static async Task BotOnMessageReceived(Message message, UpdateType type)
    {
        switch (message.Text)
        {
            case "/start":
                await _botClient!.SendMessage(message.Chat.Id,
                    """
                    Чем я могу вам помочь?) 
                    Я могу вам прислать текущую афишу БашОперы или подписаться на обновления афишы.

                    /playbill - получить текущую афишу.
                    /subscribe - подписаться на обновления афишы.

                    Полный список команд можно получить через команду /help
                    """);
                break;

            case "/forceupdate":
                await TryExecute(async () =>
                {
                    var newShows = await GetNewShows();
                    var answer = newShows.Any()
                        ? "Появились новые спектакли:\n" + string.Join('\n', newShows.Select((x, index) => $"{index + 1}. {x}"))
                        : "Новых спектаклей в БашОпере - нет :C";

                    await _botClient!.SendMessage(message.Chat.Id, answer);
                    await SaveNewShows(newShows);
                }, message.Chat.Id);

                break;
            
            case "/subscribe":
                await TryExecute(async () =>
                {
                    var cts = new CancellationTokenSource();
                    var notRunning = _ctsDictionaryForRunningTask.TryAdd(message.Chat.Id, cts);

                    if (notRunning)
                    {
                        await _botClient!.SendMessage(message.Chat.Id, "Ну все, иду <s>сталкерить</s> следить за премьерами БашОперы", parseMode: ParseMode.Html);
                    
                        // отправим сначала текущую афишу для пользователя - чтобы он знал, что уже там есть.
                        {
                            var parsedShows =  await ParseShows();
                            var answer = "На сегодня на афише БашОперы представлены следующие спектакли:\n" + string.Join('\n', parsedShows.Select((x, index) => $"{index + 1}. {x}"));
                            await _botClient!.SendMessage(message.Chat.Id, answer);
                        }
                    
                        _ = Task.Run(async () =>
                        {
                            while (cts.IsCancellationRequested == false)
                            { 
                                var newShows = await GetNewShows();

                                if (!newShows.Any())
                                    continue;

                                var answer = "Появились новые спектакли:\n" + string.Join('\n', newShows.Select((x, index) => $"{index + 1}. {x}"));
                                await _botClient!.SendMessage(message.Chat.Id, answer);
                                await SaveNewShows(newShows);
                                Thread.Sleep(1000);
                            }
                        });
                    }
                    else
                    {
                        await _botClient!.SendMessage(message.Chat.Id, "Я уже слежу за афишей для тебя) Вернусь с обновлениям, как только они появятся");
                    }
                }, message.Chat.Id);
                
                break;
            
            case "/unsubscribe":
                await TryExecute(async () =>
                {
                    var hasRunningTask = _ctsDictionaryForRunningTask.TryGetValue(message.Chat.Id, out var runningCts);

                    if (hasRunningTask)
                    {
                        runningCts!.Cancel();
                        _ctsDictionaryForRunningTask.Remove(message.Chat.Id, out var _);
                        await _botClient!.SendMessage(message.Chat.Id, "Ну вот, отписался :c\nТеперь точно пропустишь премьеру Щелкунчика...");
                    }
                    else
                    {
                        await _botClient!.SendMessage(message.Chat.Id, "Чел, ты даже еще не подписывался. От чего ты отписаться думал? ))");
                    }
                }, message.Chat.Id);
                
                break;
            
            default:
                await _botClient!.SendMessage(message.Chat.Id, "Я тебя не понимать... Выбери какую-либо команду");
                break;
        }
    }

    #region Работа со спектаклями

    private static async Task SaveNewShows(List<ShowDto> newShows)
    {
        using var scope = _host!.Services.CreateScope();
        var showRepository = scope.ServiceProvider.GetService<IShowRepository>();
        var cancellationToken = new CancellationTokenSource().Token;
        await showRepository!.AddRange(newShows, cancellationToken);
    }

    private static async Task<List<ShowDto>> GetNewShows()
    {
        var parsedShows = await ParseShows();

        using var scope = _host!.Services.CreateScope();
        var showRepository = scope.ServiceProvider.GetService<IShowRepository>();
        var cancellationToken = new CancellationTokenSource().Token;
        // TODO не эффективно тянуть все записи из БД. Лучше в БД маленькую пачку и возвращать из них ту часть, которой нет в БД.
        var existingShows = await showRepository!.GetAll(cancellationToken);
        var newShows = DetectNewShows(parsedShows, existingShows);
        return newShows;
    }

    private static async Task<IReadOnlyCollection<ShowDto>> ParseShows()
    {
        var client = CreateHttpClient();
        IHtmlContentFetcher contentFetcher = new HtmlContentFetcher(client);
        var htmlContent = await contentFetcher.GetHtmlContentAsync("https://www.bashopera.ru/affiche/");

        IPlaybillParser parser = new PlaybillParser();
        var parsedShows = parser.ParseShows(htmlContent);
        return parsedShows;
    }

    private static List<ShowDto> DetectNewShows(IReadOnlyCollection<ShowDto> parsedShows, IReadOnlyCollection<Show> existingShows)
    {
        /* TODO вообще в теории это не лучший способ определить новое шоу или нет
         Проблема в том, что в теории у существующего спектакля может поменяться время.
         и получается если я успею выгрузить спектакль до редактирования
         и попытаюсь выгрузить его после редактирования
         то в бд сохранится два РАЗНЫХ спектакля. Отличатся будут только временем
         
         Поэтому кажется, что нужно расширить ентити модель спектаклей, добавив им некий ExternalId
         возможно нужно сделать его уникальным (проверку такую повесить).
         И получается уже анализировать:
         - этот спарщенный спектакль уже есть в бд или нет
            - если уже есть - то проверить, изменилось ли время
                - если изменилось - то апдейт времени
                - если не изменилось - то ничего
            - если в бд нету - то точно добавляем
        */
        
        return parsedShows
            .Where(parsedShow => existingShows.
                Any(IsSameShow(parsedShow)) == false)
            .ToList();
    }

    private static Func<Show, bool> IsSameShow(ShowDto parsedShow)
    {
        return existingShow => existingShow.Performance.Name == parsedShow.PerformanceDto.Name && existingShow.ShowTime == parsedShow.ShowTime;
    }
    
    private static async Task TryExecute(Func<Task> action, long chatId)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await _botClient!.SendMessage(chatId, "Что-то пошло не так...");
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

    #region Системные методы

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets<Program>(optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<BashOperaDbContext>(options => 
                    options
                        .UseNpgsql(context.Configuration.GetConnectionString("Postgres"))
                        .UseSnakeCaseNamingConvention());
                services.AddTransient<IPerformanceRepository, PerformanceRepository>();
                services.AddTransient<IShowRepository, ShowRepository>();
            });

    // TODO когда дойду до переделки проекта под нормальный с DI - лучше использовать IHttpClientFactory
    private static HttpClient CreateHttpClient()
    {
        var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new HttpRetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3
            })
            .Build();

        var socketHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15)
        };
#pragma warning disable EXTEXP0001
        var resilienceHandler = new ResilienceHandler(retryPipeline)
#pragma warning restore EXTEXP0001
        {
            InnerHandler = socketHandler,
        };

        return new HttpClient(resilienceHandler);
    }

    #endregion

    
}