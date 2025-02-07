using JetBrains.Annotations;
namespace ParserConsole.Services.Interfaces;


/// <summary>
/// Парсер афиши.
/// </summary>
public interface IPlaybillParser
{
    /// <summary>
    /// Парсит html-контент афиши.
    /// </summary>
    /// <param name="htmlContent">Html-контент афиши.</param>
    /// <returns>Коллекция спектаклей со местом и временем показа.</returns>
    IReadOnlyCollection<ShowDto> ParseShows(string htmlContent);
}


/// <summary>
/// Спектакль.
/// </summary>
/// <param name="Name">Название спектакля.</param>
public record PerformanceDto([UsedImplicitly] string Name);

/// <summary>
/// Представление (сеанс показа спектакля).
/// </summary>
/// <param name="PerformanceDto">Спектакль, который будет показан.</param>
/// <param name="ShowTime">Дата и время начала представления.</param>
/// <param name="Location">Место проведения представления.</param>
public record ShowDto (
    [UsedImplicitly] PerformanceDto PerformanceDto, 
    [UsedImplicitly] DateTime ShowTime, 
    [UsedImplicitly] string Location);