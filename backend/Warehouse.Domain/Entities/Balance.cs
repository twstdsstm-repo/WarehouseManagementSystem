namespace Warehouse.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class Balance
    {
        public int Id { get; set; }

        [Required]
        public int ResourceId { get; set; }

        public Resource? Resource { get; set; }

        [Required]
        public int UnitOfMeasurementId { get; set; }

        public UnitOfMeasurement? UnitOfMeasurement { get; set; }

        [Required]
        public decimal Quantity { get; set; }
    }
}
