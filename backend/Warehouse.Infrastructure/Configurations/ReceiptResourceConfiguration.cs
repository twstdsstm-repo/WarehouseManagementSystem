using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Configurations
{
    public class ReceiptResourceConfiguration : IEntityTypeConfiguration<ReceiptResource>
    {
        public void Configure(EntityTypeBuilder<ReceiptResource> builder)
        {
            builder.ToTable("ReceiptResources");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                   .IsRequired();

            builder.HasOne(x => x.Resource)
                   .WithMany(x => x.ReceiptResources)
                   .HasForeignKey(x => x.ResourceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UnitOfMeasurement)
                   .WithMany(x => x.ReceiptResources)
                   .HasForeignKey(x => x.UnitOfMeasurementId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceiptDocument)
                   .WithMany(x => x.ReceiptResources)
                   .HasForeignKey(x => x.ReceiptDocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ReceiptResources_Quantity_Positive", "\"Quantity\" > 0");
            });
        }
    }
}
