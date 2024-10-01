using System.Text;
using FluentAssertions;
using ParserConsole;
using RichardSzalay.MockHttp;

namespace Tests;

public class HtmlContentFetcherTests
{
    public HtmlContentFetcherTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    
    [Fact]
    public async Task GetHtmlContentAsync_ReturnsHtmlContent()
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
}