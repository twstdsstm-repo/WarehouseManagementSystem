namespace Warehouse.Domain.Entities;

using System.ComponentModel.DataAnnotations;

public class ReceiptDocument
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }

    public ICollection<ReceiptResource> ReceiptResources { get; set; } = new List<ReceiptResource>();
}
