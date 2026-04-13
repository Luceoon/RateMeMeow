using Microsoft.EntityFrameworkCore;
using RMM.Models;

namespace RMM.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(rl => rl.User)
            .HasForeignKey(rl => rl.UserId);

        modelBuilder.Entity<Review>()
            .HasOne(rl => rl.User)
            .WithMany(r => r.Reviews)
            .HasForeignKey(u => u.UserId);

        modelBuilder.Entity<Review>(b =>
        {
            b.HasIndex(r => new { r.UserId, r.WasRated, r.SubmittedAt })
                .HasFilter("\"WasRated\" = true");
        });

        base.OnModelCreating(modelBuilder);
    }
}
