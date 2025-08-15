namespace Warehouse.Application.Models.Filters;

public class BalanceFilter
{
    public List<int>? ResourceIds { get; set; }
    public List<int>? UnitOfMeasurementIds { get; set; }
}