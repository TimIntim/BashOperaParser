using FluentAssertions;
using ParserConsole.Services;
using ParserConsole.Services.Interfaces;

namespace Tests;

public class PlaybillParserTests
{
    [Fact]
    public Task ParseShows_ReturnsCorrectShows()
    {
        // Arrange
        var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
</head>
<body>
<div class=""item content fm-10 fs-0 fs-6"" id=""bx_3218110189_15053"">
    <div class=""col dates"">
        <div class=""date""><strong>21 ноября</strong></div>
        <div class=""weekday"">четверг</div>
    </div>
    <div class=""col names"">
        <div class=""name""><a href=""/repertoire/concert/15052/"">Северные амуры (концертное исполнение)</a></div>
        <div class=""tags"">
            <ul class=""b-tags"">
                <li>12+</li>
            </ul>
        </div>
    </div>
    <div class=""col staff""></div>
    <div class=""col actions"">
        <div class=""hall""><strong><a class=""a-hall about-place"" href=""/tickets/halls/234/?template=blank"">Большой зал</a>, 19:00</strong></div>
        <div class=""price""></div>
        <div class=""buy"">
            <a href=""/repertoire/concert/15052/15053/"" onclick=""yaCounter37201245.reachGoal('kassy-btn');"" class=""b-byu-btn"">
                Билеты
                <img src=""/bitrix/templates/responsive/img/ticket.svg"">
            </a>
        </div>
    </div>
</div>

<div class=""item content fm-10 fs-0 fs-3"" id=""bx_3218110189_15026"">
    <div class=""col dates"">
        <div class=""date""><strong>22 ноября</strong></div>
        <div class=""weekday"">пятница</div>
    </div>
    <div class=""col names"">
        <div class=""name""><a href=""/repertoire/operetta/291/"">Весёлая вдова</a></div>
        <div class=""tags"">
            <ul class=""b-tags"">
                <li class=""composer"">Франц Легар</li>
                <li>12+</li>
            </ul>
        </div>
    </div>
    <div class=""col staff""></div>
    <div class=""col actions"">
        <div class=""hall""><strong><a class=""a-hall about-place"" href=""/tickets/halls/234/?template=blank"">Большой зал</a>, 19:00</strong></div>
        <div class=""price""></div>
        <div class=""buy"">
            <img src=""/bitrix/templates/responsive/img/pushkin.svg"" class=""pushkin"" title=""Доступно по Пушкинской карте"">
            <a href=""/repertoire/operetta/291/15026/"" onclick=""yaCounter37201245.reachGoal('kassy-btn');"" class=""b-byu-btn"">
                Билеты
                <img src=""/bitrix/templates/responsive/img/ticket.svg"">
            </a>
        </div>
    </div>
</div>
</body>
        ";
        IPlaybillParser sut = new PlaybillParser(); 
        
        // Act
        var shows = sut.ParseShows(htmlContent);

        // Assert
        shows.Should().HaveCount(2);
        return Verify(shows);
    }
}