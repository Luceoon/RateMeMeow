using Microsoft.EntityFrameworkCore;
using RMM.Models;

namespace RMM.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ReviewLink> ReviewLinks => Set<ReviewLink>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.ReviewLinks)
            .WithOne(rl => rl.User)
            .HasForeignKey(rl => rl.UserId);

        modelBuilder.Entity<ReviewLink>()
            .HasOne(rl => rl.Review)
            .WithOne(r => r.ReviewLink)
            .HasForeignKey<Review>(r => r.ReviewLinkId);

        base.OnModelCreating(modelBuilder);
    }
}
