using Microsoft.EntityFrameworkCore;
using ParserConsole.Services.Interfaces;

namespace ParserConsole;

public interface IShowRepository
{
    Task<IReadOnlyCollection<Show>> GetAll(CancellationToken cancellationToken);
    Task AddRange(IReadOnlyCollection<ShowDto> shows, CancellationToken cancellationToken);
}

public class ShowRepository : IShowRepository
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
        var performanceRepository = new PerformanceRepository();
        foreach (var show in shows)
        {
            if (!performances.TryGetValue(show.PerformanceDto.Name, out var performance))
            {
                performance = await performanceRepository.GetByName(show.PerformanceDto.Name, cancellationToken);
                if (performance == null)
                {
                    performance = new Performance() { Name = show.PerformanceDto.Name };
                }

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