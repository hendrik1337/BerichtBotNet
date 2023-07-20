using BerichtBotNet.Data;
using Microsoft.EntityFrameworkCore;

namespace BerichtBotNet.Models;

public class BerichtBotContext : DbContext
{
    public DbSet<Apprentice> Apprentices { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<SkippedWeeks> SkippedWeeks { get; set; }

    public BerichtBotContext(DbContextOptions options) : base(options)
    {
    }

    public BerichtBotContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseNpgsql(Environment.GetEnvironmentVariable("PostgreSQLBerichtBotConnection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Apprentice>().HasIndex(u => u.DiscordUserId).IsUnique();
        modelBuilder.Entity<Group>().HasIndex(u => u.Name).IsUnique();
    }
}
