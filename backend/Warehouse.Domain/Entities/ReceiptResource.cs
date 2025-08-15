using System.ComponentModel.DataAnnotations;

namespace Warehouse.Domain.Entities
{
    public class ReceiptResource
    {
        public int Id { get; set; }
        public int ReceiptDocumentId { get; set; }
        public int ResourceId { get; set; }
        public int UnitOfMeasurementId { get; set; }
        public decimal Quantity { get; set; }

    public virtual Resource? Resource { get; set; }
    public virtual UnitOfMeasurement? UnitOfMeasurement { get; set; }
    public virtual ReceiptDocument? ReceiptDocument { get; set; }
    }
}
