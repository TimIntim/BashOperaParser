using System.Text;
using Microsoft.Extensions.Http.Resilience;
using ParserConsole.Services;
using ParserConsole.Services.Interfaces;
using Polly;

namespace ParserConsole;

internal static class Program
{
    static async Task Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var client = CreateHttpClient();
        IHtmlContentFetcher contentFetcher = new HtmlContentFetcher(client);
        try
        {
            var htmlContent = await contentFetcher.GetHtmlContentAsync("https://www.bashopera.ru/affiche/");

            IPlaybillParser parser = new PlaybillParser();
            var parsedShows = parser.ParseShows(htmlContent);

            IShowRepository showRepository = new ShowRepository();
            var cancellationToken = new CancellationTokenSource().Token;
            // TODO не эффективно тянуть все записи из БД. Лучше в БД маленькую пачку и возвращать из них ту часть, которой нет в БД.
            var existingShows = await showRepository.GetAll(cancellationToken);
            var newShows = DetectNewShows(parsedShows, existingShows);
            await showRepository.AddRange(newShows, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка: {e.Message}");
        }
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
}