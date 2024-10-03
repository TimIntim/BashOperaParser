namespace ParserConsole.Services.Interfaces;

internal interface IPlaybillParser
{
    IReadOnlyCollection<Show> ParseShows(string htmlContent);
}