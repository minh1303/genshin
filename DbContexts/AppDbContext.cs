using genshin.Models;
using Microsoft.EntityFrameworkCore;

namespace genshin.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Element>().HasIndex(x => x.Name).IsUnique();
    }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Element> Elements { get; set; }
    public DbSet<User> Users { get; set; }
}
