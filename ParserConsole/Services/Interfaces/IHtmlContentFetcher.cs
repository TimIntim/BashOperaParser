namespace ParserConsole.Services.Interfaces;

/// <summary>
/// Html-парсер
/// </summary>
public interface IHtmlContentFetcher
{
    /// <summary>
    /// Возвращает контент страницы по его url-у.
    /// </summary>
    /// <param name="url">Url страницы.</param>
    Task<string> GetHtmlContentAsync(string url);
}