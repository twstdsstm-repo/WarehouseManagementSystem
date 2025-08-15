using System.ComponentModel.DataAnnotations;

namespace Warehouse.Domain.Entities
{
    public class Resource
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public bool IsArchived { get; set; } = false;

        public ICollection<Balance> Balances { get; set; } = new List<Balance>();
        public ICollection<ReceiptResource> ReceiptResources { get; set; } = new List<ReceiptResource>();
        public ICollection<ShipmentResource> ShipmentResources { get; set; } = new List<ShipmentResource>();
        
    }
}
