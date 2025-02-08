using Microsoft.EntityFrameworkCore;

namespace ParserConsole.Repositories;

public class PerformanceRepository : IPerformanceRepository
{
    public async Task<Performance?> GetByName(string name, CancellationToken cancellationToken)
    {
        await using var context = new BashOperaDbContext();
        
        return await context.Performances.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }
}

public interface IPerformanceRepository
{
    Task<Performance?> GetByName(string name, CancellationToken cancellationToken);
}
