using System.Text;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace ParserConsole;

internal static class Program
{
    static async Task Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var client = CreateHttpClient();
        IHtmlParser parser = new HtmlParser(client);
        try
        {
            var htmlContent = await parser.GetHtmlContentAsync("https://www.bashopera.ru/affiche/");
            Console.WriteLine(htmlContent);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка: {e.Message}");
        }
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

internal record Performance(string Name, IReadOnlyCollection<Tag> Tags);
internal record Tag(string Name);

internal enum Venue
{
    LargeHall,
    SmallHall,
}

internal record Show (Performance Performance, DateTime ShowTime, Venue Location);