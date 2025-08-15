using System.ComponentModel.DataAnnotations;

namespace Warehouse.Domain.Entities;

public class ShipmentResource
{
    public int Id { get; set; }

    
    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }

    
    public int UnitOfMeasurementId { get; set; }
    public UnitOfMeasurement? UnitOfMeasurement { get; set; }

    
    public int ShipmentDocumentId { get; set; }
    public ShipmentDocument? ShipmentDocument { get; set; }

    public decimal Quantity { get; set; }
}

