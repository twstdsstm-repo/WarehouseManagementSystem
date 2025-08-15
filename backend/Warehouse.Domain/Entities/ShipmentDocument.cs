using System.ComponentModel.DataAnnotations;

namespace Warehouse.Domain.Entities;

public class ShipmentDocument
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required]
    public int ClientId { get; set; }

    public DateTimeOffset Date { get; set; }

    public bool State { get; set; }

    public ICollection<ShipmentResource> ShipmentResources { get; set; } = new List<ShipmentResource>();
}