using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ParserConsole;

public sealed class BashOperaDbContext : DbContext
{
    public BashOperaDbContext(DbContextOptions<BashOperaDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    public DbSet<Show> Shows { get; set; }
    public DbSet<Performance> Performances { get; set; }
    
    public BashOperaDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO переделать на отдельные Configuration под каждые сущности
        modelBuilder.Entity<Show>()
            .Property(x => x.Location)
            .HasMaxLength(128);
        
        modelBuilder.Entity<Show>()
            .Property(s => s.ShowTime)
            .HasColumnType("timestamp");

        modelBuilder.Entity<Performance>()
            .Property(x => x.Name)
            .HasMaxLength(128);
    }
}

public class BaseEntity<TIdentity> where TIdentity : struct
{
    public TIdentity Id { get; set; }
}

/// <summary>
/// Спектакль.
/// </summary>
public class Performance : BaseEntity<int>
{
    /// <summary>
    /// Название спектакля.
    /// </summary>
    public required string Name { get; init; }
}

/// <summary>
/// Представление (сеанс показа спектакля).
/// </summary>
public class Show : BaseEntity<int>
{
    /// <summary>
    /// Спектакль, который будет показан.
    /// </summary>
    public required Performance Performance { get; init; }
    
    /// <summary>
    /// Id спектакля.
    /// </summary>
    public int PerformanceId { get; init; }
    
    /// <summary>
    /// Дата и время начала представления.
    /// </summary>
    public DateTime ShowTime { get; init; }
    
    /// <summary>
    /// Место проведения представления.
    /// </summary>
    public required string Location { get; init; }
}