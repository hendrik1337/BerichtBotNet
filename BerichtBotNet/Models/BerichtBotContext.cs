using BerichtBotNet.Data;
using Microsoft.EntityFrameworkCore;

namespace BerichtBotNet.Models;

public class BerichtBotContext : DbContext
{
    public DbSet<Apprentice> Apprentices { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Log> Logs { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder contextOptionsBuilder)
    {
        // Connection String to Connect to the Database
        contextOptionsBuilder
            .UseLazyLoadingProxies()
            .UseNpgsql(Environment.GetEnvironmentVariable("PostgreSQLBerichtBotConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Apprentice>().HasIndex(u => u.DiscordUserId).IsUnique();
    }
}