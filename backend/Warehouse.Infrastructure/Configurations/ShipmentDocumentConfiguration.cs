using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Configurations
{
    public class ShipmentDocumentConfiguration : IEntityTypeConfiguration<ShipmentDocument>
    {
        public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Number).IsUnique();

            builder.Property(x => x.Number)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.State)
                .IsRequired();

            builder.Property(x => x.ClientId)
                .IsRequired();
        }
    }
}
