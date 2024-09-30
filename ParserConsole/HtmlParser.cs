using System.Text;

namespace ParserConsole;

public class HtmlParser(HttpClient client) : IHtmlParser
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
            throw new Exception($"Ошибка получения HTML: {e.Message}");
        }
    }
}