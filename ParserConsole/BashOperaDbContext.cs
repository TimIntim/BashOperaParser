using Microsoft.EntityFrameworkCore;

namespace ParserConsole;

public sealed class BashOperaDbContext : DbContext
{
    public DbSet<Show> Shows { get; set; }
    public DbSet<Performance> Performances { get; set; }
    
    public BashOperaDbContext()
    {
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5432;Database=bash_opera_db;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Show>()
            .Property(x => x.Location)
            .HasMaxLength(128);

        modelBuilder.Entity<Performance>()
            .Property(x => x.Name)
            .HasMaxLength(128);
    }
}

public class Show : BaseEntity<int>
{
    public Performance Performance { get; set; }
    public int PerformanceId { get; set; }
    public DateTime ShowTime { get; set; }
    public string Location { get; set; }
}

public class BaseEntity<TIdentity> where TIdentity : struct
{
    public TIdentity Id { get; set; }
}

public class Performance : BaseEntity<int>
{
    public string Name { get; set; }
}