using Warehouse.Domain.Entities;

namespace Warehouse.Application.Interfaces;

public interface IShipmentResourceService
{
    Task<List<ShipmentResource>> GetAllAsync();
    Task<ShipmentResource?> GetByIdAsync(int id);
    Task<int> CreateAsync(ShipmentResource resource);
    Task<bool> UpdateAsync(int id, ShipmentResource resource);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateBalance(int resourceId, int unitId, decimal delta);
    Task<bool> IsArchived(int resourceId, int unitId);
}
