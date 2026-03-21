using Microsoft.EntityFrameworkCore;

namespace FlowCore.OrderService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("case_order");
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.CustomerId).IsRequired();
            e.Property(o => o.Amount).HasPrecision(18, 2);
            e.Property(o => o.State).HasMaxLength(20).HasDefaultValue("created");
        });
    }
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string State { get; set; } = "created";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
