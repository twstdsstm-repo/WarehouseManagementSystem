using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Configurations;

public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("Balances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
               .IsRequired()
               .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Resource)
        .WithMany(x => x.Balances)
        .HasForeignKey(x => x.ResourceId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UnitOfMeasurement)
        .WithMany(x => x.Balances)
        .HasForeignKey(x => x.UnitOfMeasurementId)
        .OnDelete(DeleteBehavior.Restrict);
       
        
        builder.HasIndex(b => new { b.ResourceId, b.UnitOfMeasurementId })
            .IsUnique();

        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Balances_Quantity_NonNegative", "\"Quantity\" >= 0");
        });
    }
}