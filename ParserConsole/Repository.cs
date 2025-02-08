using Microsoft.EntityFrameworkCore;
using ParserConsole.Services.Interfaces;

namespace ParserConsole;

public interface IRepository
{
    Task<IReadOnlyCollection<Show>> GetAll(CancellationToken cancellationToken);
    Task AddRange(IReadOnlyCollection<ShowDto> shows, CancellationToken cancellationToken);
}

public class Repository : IRepository
{
    public async Task<IReadOnlyCollection<Show>> GetAll(CancellationToken cancellationToken)
    {
        await using var context = new BashOperaDbContext();

        return await context.Shows
            .Include(x => x.Performance)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRange(IReadOnlyCollection<ShowDto> shows, CancellationToken cancellationToken)
    {
        await using var context = new BashOperaDbContext();
        
        var showEntities = new List<Show>();
        var performances = new Dictionary<string, Performance>();
        foreach (var show in shows)
        {
            // TODO проверка идет только в рамках текущей пачки. Если в БД уже есть спектакль с таким же именем, то он будет добавлен в БД, как дубль.
            if (!performances.TryGetValue(show.PerformanceDto.Name, out var performance))
            {
                performance = new Performance() { Name = show.PerformanceDto.Name };
                performances.Add(show.PerformanceDto.Name, performance);
            }
            
            // TODO по-хорошему лучше маппинг либо вынести в отдельный маппер класс, или в дто добавить метод ToEntity<TEntity>
            var showEntity = new Show
            {
                Performance = performance,
                ShowTime = show.ShowTime,
                Location = show.Location
            };
            showEntities.Add(showEntity);
        }
        context.UpdateRange(showEntities);
        await context.SaveChangesAsync(cancellationToken);
    }
}