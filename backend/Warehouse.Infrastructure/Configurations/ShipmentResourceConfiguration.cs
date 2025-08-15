using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

public class ShipmentResourceConfiguration : IEntityTypeConfiguration<ShipmentResource>
{
    public void Configure(EntityTypeBuilder<ShipmentResource> b)
    {
        b.ToTable("ShipmentResources");
        b.HasKey(x => x.Id);

        b.Property(x => x.Quantity)
            .IsRequired()
            .HasColumnType("numeric(18,3)");

        
        b.HasOne(x => x.ShipmentDocument)
            .WithMany(d => d.ShipmentResources)
            .HasForeignKey(x => x.ShipmentDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        
        b.HasOne(x => x.Resource)
            .WithMany()
            .HasForeignKey(x => x.ResourceId) 
            .OnDelete(DeleteBehavior.Restrict);

        
        b.HasOne(x => x.UnitOfMeasurement)
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasurementId) 
            .OnDelete(DeleteBehavior.Restrict);

        
        b.HasIndex(x => new { x.ShipmentDocumentId, x.ResourceId, x.UnitOfMeasurementId })
            .IsUnique();
    }
}
