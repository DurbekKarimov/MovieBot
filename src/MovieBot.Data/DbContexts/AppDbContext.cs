using Microsoft.EntityFrameworkCore;
using MovieBot.Domain.Entities;

namespace MovieBot.Data.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<BotAdmin> BotAdmins { get; set; }
    public DbSet<SubscriptionChannel> SubscriptionChannels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}
