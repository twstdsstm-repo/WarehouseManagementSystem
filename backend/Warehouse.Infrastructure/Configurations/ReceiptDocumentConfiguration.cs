using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Configurations;

public class ReceiptDocumentConfiguration : IEntityTypeConfiguration<ReceiptDocument>
{
    public void Configure(EntityTypeBuilder<ReceiptDocument> builder)
    {
        builder.ToTable("ReceiptDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Date)
               .IsRequired();

        builder.HasMany(x => x.ReceiptResources)
               .WithOne()
               .HasForeignKey(r => r.ReceiptDocumentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}