﻿using System.Text;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace ParserConsole;

internal static class Program
{
    static async Task Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var client = CreateHttpClient();
        var parser = new HtmlParser(client);
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