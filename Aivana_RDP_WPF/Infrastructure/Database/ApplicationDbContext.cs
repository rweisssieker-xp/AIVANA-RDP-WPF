using Microsoft.EntityFrameworkCore;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Infrastructure.Database;

/// <summary>
/// Application database context for Entity Framework Core.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public DbSet<ConnectionProfile> ConnectionProfiles { get; set; }
    public DbSet<SessionHistory> SessionHistories { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ConnectionProfile
        modelBuilder.Entity<ConnectionProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ServerAddress).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Port).HasDefaultValue(3389);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Domain).HasMaxLength(100);
            entity.Property(e => e.GroupName).HasMaxLength(100);
            entity.Property(e => e.Tags).HasDefaultValue("[]");
            entity.Property(e => e.Settings).HasDefaultValue("{}");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ConnectionCount).HasDefaultValue(0);

            // Indexes
            entity.HasIndex(e => e.IsFavorite);
            entity.HasIndex(e => e.GroupName);
        });

        // Configure SessionHistory
        modelBuilder.Entity<SessionHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.ConnectedAt).IsRequired();

            // Foreign key relationship
            entity.HasOne(e => e.ConnectionProfile)
                .WithMany(c => c.SessionHistories)
                .HasForeignKey(e => e.ConnectionProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ConnectionProfileId);
            entity.HasIndex(e => e.ConnectedAt);
        });
    }
}

