using Microsoft.EntityFrameworkCore;

namespace FlowCore.AuditService.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");
        modelBuilder.Entity<AuditEntry>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            e.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
            e.Property(a => a.Action).HasMaxLength(100).IsRequired();
            e.Property(a => a.PerformedBy).HasMaxLength(100).IsRequired();
            e.HasIndex(a => new { a.EntityType, a.EntityId });
        });
    }
}

public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
}
