namespace ParserConsole;

/// <summary>
/// Html-парсер
/// </summary>
public interface IHtmlParser
{
    /// <summary>
    /// Возвращает контент страницы по его url-у.
    /// </summary>
    /// <param name="url">Url страницы.</param>
    Task<string> GetHtmlContentAsync(string url);
}