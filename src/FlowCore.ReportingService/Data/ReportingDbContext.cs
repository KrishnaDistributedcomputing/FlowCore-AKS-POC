using Microsoft.EntityFrameworkCore;

namespace FlowCore.ReportingService.Data;

public class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options) { }

    public DbSet<ReportSummary> ReportSummaries => Set<ReportSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reporting");
        modelBuilder.Entity<ReportSummary>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.MetricName).HasMaxLength(100);
        });
    }
}

public class ReportSummary
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MetricName { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
