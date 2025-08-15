using System.ComponentModel.DataAnnotations;

namespace Warehouse.Domain.Entities;

public class Client
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public bool IsArchived { get; set; } = false;

    public ICollection<ShipmentDocument> ShipmentDocuments { get; set; } = new List<ShipmentDocument>();
}