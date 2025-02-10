using Microsoft.EntityFrameworkCore;
using ParserConsole.Services.Interfaces;

namespace ParserConsole.Repositories;

public interface IShowRepository
{
    Task<IReadOnlyCollection<Show>> GetAll(CancellationToken cancellationToken);
    Task AddRange(IReadOnlyCollection<ShowDto> shows, CancellationToken cancellationToken);
}

public class ShowRepository : IShowRepository
{
    private readonly BashOperaDbContext _context;

    public ShowRepository(BashOperaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Show>> GetAll(CancellationToken cancellationToken)
    {
        return await _context.Shows
            .Include(x => x.Performance)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRange(IReadOnlyCollection<ShowDto> shows, CancellationToken cancellationToken)
    {
        var showEntities = new List<Show>();
        var performances = new Dictionary<string, Performance>();
        var performanceRepository = new PerformanceRepository(_context);
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
        _context.UpdateRange(showEntities);
        await _context.SaveChangesAsync(cancellationToken);
    }
}