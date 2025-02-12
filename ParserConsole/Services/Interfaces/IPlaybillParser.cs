using System.Globalization;
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
public record ShowDto
{
    /// <summary>
    /// Представление (сеанс показа спектакля).
    /// </summary>
    /// <param name="PerformanceDto">Спектакль, который будет показан.</param>
    /// <param name="ShowTime">Дата и время начала представления.</param>
    /// <param name="Location">Место проведения представления.</param>
    public ShowDto([UsedImplicitly] PerformanceDto PerformanceDto, 
        [UsedImplicitly] DateTime ShowTime, 
        [UsedImplicitly] string Location)
    {
        this.PerformanceDto = PerformanceDto;
        this.ShowTime = ShowTime;
        this.Location = Location;
    }

    /// <summary>Спектакль, который будет показан.</summary>
    public PerformanceDto PerformanceDto { get; init; }

    /// <summary>Дата и время начала представления.</summary>
    public DateTime ShowTime { get; init; }

    /// <summary>Место проведения представления.</summary>
    public string Location { get; init; }

    public override string ToString()
    {
        return $"Спектакль \"{PerformanceDto.Name}\", дата: {ShowTime:dd/MM/yyyy HH:mm}, место: {Location}";
    }
}