using DocNestApp.Domain.Documents;
using DocNestApp.Domain.Reminders;
using Microsoft.EntityFrameworkCore;

namespace DocNestApp.Infrastructure.Database;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>(b =>
        {
            b.ToTable("documents");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50);

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();

            b.HasIndex(x => new { x.UserId, x.SubjectId });
            b.HasIndex(x => new { x.UserId, x.ExpiresOn });
            
            b.Property(x => x.FileKey).HasMaxLength(300);
            b.Property(x => x.OriginalFileName).HasMaxLength(255);
            b.Property(x => x.ContentType).HasMaxLength(100);
            b.Property(x => x.SizeBytes);
        });
        
        modelBuilder.Entity<Reminder>(b =>
        {
            b.ToTable("reminders");
            b.HasKey(x => x.Id);

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.DueAtUtc).IsRequired();
            b.Property(x => x.ExpiresOn).IsRequired();
            b.Property(x => x.DaysBefore).IsRequired();

            // Idempotency: one reminder per doc per policy
            b.HasIndex(x => new { x.DocumentId, x.DaysBefore }).IsUnique();

            b.HasIndex(x => new { x.UserId, x.DueAtUtc });
            b.HasIndex(x => x.DispatchedAtUtc);
        });
    }
}