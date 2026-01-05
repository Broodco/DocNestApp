using DocNestApp.Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace DocNestApp.Infrastructure.Database;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();

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
    }
}