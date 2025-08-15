namespace Warehouse.Application.Models.Filters;

public class ReceiptDocumentFilter
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string>? Numbers { get; set; }
    public List<int>? ResourceIds { get; set; }
    public List<int>? UnitOfMeasurementIds { get; set; }
}