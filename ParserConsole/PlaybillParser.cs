using System.Globalization;
using HtmlAgilityPack;

namespace ParserConsole;

internal class PlaybillParser : IPlaybillParser
{
    public IReadOnlyCollection<Show> ParseShows(string htmlContent)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        
        var showNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'item content')]");
        var shows = new List<Show>();
        
        foreach (var showNode in showNodes)
        {
            var nameNode = showNode.SelectSingleNode(".//div[contains(@class, 'col names')]//a");
            var performanceName = nameNode?.InnerText.Trim();

            var venueNode = showNode.SelectSingleNode(".//div[contains(@class, 'col actions')]//a");
            var venueName = venueNode?.InnerText.Trim();

            var dateNode = showNode.SelectSingleNode(".//div[contains(@class, 'col dates')]//strong");
            var dateText = dateNode?.InnerText.Trim();
            var timeText = venueNode?.NextSibling.InnerText.Replace(",", "").Trim();
            var showTime = TryParseShowDateTime(dateText, timeText);


            if (string.IsNullOrWhiteSpace(performanceName) ||
                string.IsNullOrWhiteSpace(venueName) ||
                showTime is null)
            {
                //TODO сюда надо добавить логи
                continue;
            }
            
            var performance = new Performance(performanceName);
            var show = new Show(performance, showTime.Value, venueName);
            shows.Add(show);
        }

        return shows;
    }
    
    private DateTime? TryParseShowDateTime(string? dateText, string? timeText)
    {
        if (string.IsNullOrWhiteSpace(dateText) || string.IsNullOrWhiteSpace(timeText))
            return null;
        
        // Формат даты: 4 октября
        // Формат времени: 19:00 (без пробелов)
        var dateFormat = "d MMMM";
        var timeFormat = "HH:mm";

        if (!DateTime.TryParseExact(dateText, dateFormat, 
                new CultureInfo("ru-RU"), 
                DateTimeStyles.None, 
                out var parsedDate))
        {
            return null;
        }

        if (!DateTime.TryParseExact(timeText.Replace(" ", ""), timeFormat, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var parsedTime))
        {
            return null;
        }

        return new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, parsedTime.Hour, parsedTime.Minute, 0);
    }
}