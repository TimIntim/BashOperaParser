using Microsoft.EntityFrameworkCore;

namespace ParserConsole.Repositories;

public class PerformanceRepository : IPerformanceRepository
{
    private readonly BashOperaDbContext _context;

    public PerformanceRepository(BashOperaDbContext context)
    {
        _context = context;
    }
    public async Task<Performance?> GetByName(string name, CancellationToken cancellationToken)
    {
        return await _context.Performances.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }
}

public interface IPerformanceRepository
{
    Task<Performance?> GetByName(string name, CancellationToken cancellationToken);
}
