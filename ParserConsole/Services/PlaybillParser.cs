﻿using System.Globalization;
using HtmlAgilityPack;
using ParserConsole.Services.Interfaces;

namespace ParserConsole.Services;

public class PlaybillParser : IPlaybillParser
{
    public IReadOnlyCollection<ShowDto> ParseShows(string htmlContent)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        
        var showNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'item content')]");
        var shows = new List<ShowDto>();
        
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
            
            var performance = new PerformanceDto(Name: performanceName);
            var show = new ShowDto(performance, showTime.Value, venueName);
            shows.Add(show);
        }

        return shows;
    }
    
    private DateTime? TryParseShowDateTime(string? dateText, string? timeText)
    {
        if (string.IsNullOrWhiteSpace(dateText) || string.IsNullOrWhiteSpace(timeText))
            return null;
        
        // Формат даты: 4 октября 19:00
        const string dateFormat = "d MMMM HH:mm";

        if (!DateTime.TryParseExact($"{dateText} {timeText}", dateFormat, 
                new CultureInfo("ru-RU"), 
                DateTimeStyles.None, 
                out var parsedDate))
        {
            return null;
        }

        return parsedDate;
    }
}