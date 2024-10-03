using System.Text;
using ParserConsole.Services.Interfaces;

namespace ParserConsole.Services;

public class HtmlContentFetcher(HttpClient client) : IHtmlContentFetcher
{
    public async Task<string> GetHtmlContentAsync(string url)
    {
        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var encoding = Encoding.GetEncoding("windows-1251");
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return encoding.GetString(bytes);
        }
        catch (HttpRequestException e)
        {
            throw new HtmlContentException($"Ошибка при запросе к {url}: {e.Message}", e);
        }
        catch (Exception e)
        {
            throw new HtmlContentException($"Неизвестная ошибка при обработке HTML с {url}: {e.Message}", e);
        }
    }
}


public class HtmlContentException : Exception
{
    public HtmlContentException(string message) : base(message) { }
    public HtmlContentException(string message, Exception innerException) : base(message, innerException) { }
}