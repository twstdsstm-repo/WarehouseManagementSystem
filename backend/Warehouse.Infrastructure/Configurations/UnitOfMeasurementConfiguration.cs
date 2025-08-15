using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Configurations
{
    public class UnitOfMeasurementConfiguration : IEntityTypeConfiguration<UnitOfMeasurement>
    {
        public void Configure(EntityTypeBuilder<UnitOfMeasurement> builder)
        {
            builder.ToTable("UnitsOfMeasurement");

            builder.HasIndex(x => x.Name).IsUnique();

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IsArchived)
                .IsRequired();
        }
    }
}
