using System.Text;
using FluentAssertions;
using ParserConsole.Services;
using ParserConsole.Services.Interfaces;
using RichardSzalay.MockHttp;

namespace Tests;

public class HtmlContentFetcherTests
{
    public HtmlContentFetcherTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    
    [Fact]
    public async Task HtmlContentFetcher_returns_correct_html_content_for_valid_url()
    {
        // Arrange
        var url = "https://www.bashopera.ru/affiche/";
        var expectedHtml = "<html><body>Hello World</body></html>";
        
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(url)
                .Respond("text/html", expectedHtml);

        var client = mockHttp.ToHttpClient();
        IHtmlContentFetcher contentFetcher = new HtmlContentFetcher(client);

        // Act
        var result = await contentFetcher.GetHtmlContentAsync(url);

        // Assert
        result.Should().Be(expectedHtml);
    }
    
    [Fact]
    public async Task Request_with_invalid_url_throws_HtmlContentException()
    {
        // Arrange
        var invalidUrl = "htp://www.bashopera.ru/affiche/";
    
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(invalidUrl)
            .Throw(new HttpRequestException("Network error"));

        var client = mockHttp.ToHttpClient();
        IHtmlContentFetcher contentFetcher = new HtmlContentFetcher(client);

        // Act
        Func<Task> act = async () => await contentFetcher.GetHtmlContentAsync(invalidUrl);

        // Assert
        await act.Should().ThrowAsync<HtmlContentException>()
            .WithMessage($"Ошибка при запросе к {invalidUrl}: Network error");
    }

    [Fact]
    public async Task Request_throws_HtmlContentException_on_unknown_error()
    {
        // Arrange
        var url = "https://www.bashopera.ru/affiche/";
    
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(url)
            .Throw(new InvalidOperationException("Unknown error"));

        var client = mockHttp.ToHttpClient();
        IHtmlContentFetcher contentFetcher = new HtmlContentFetcher(client);

        // Act
        Func<Task> act = async () => await contentFetcher.GetHtmlContentAsync(url);

        // Assert
        await act.Should().ThrowAsync<HtmlContentException>()
            .WithMessage($"Неизвестная ошибка при обработке HTML с {url}: Unknown error");
    }
}