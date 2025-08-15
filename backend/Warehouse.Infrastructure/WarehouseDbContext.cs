using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure;

public class WarehouseDbContext : DbContext
{
    public DbSet<Resource> Resources { get; set; }
    public DbSet<UnitOfMeasurement> UnitsOfMeasurement { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Balance> Balances { get; set; } = null!;
    public DbSet<ReceiptDocument> ReceiptDocuments { get; set; } = null!;
    public DbSet<ReceiptResource> ReceiptResources { get; set; } = null!;
    public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
    public DbSet<ShipmentResource> ShipmentResources { get; set; }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Resource>()
            .HasIndex(r => r.Name)
            .IsUnique();

        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}