using Warehouse.Domain.Entities;
using Warehouse.Application.Models.Filters;

namespace Warehouse.Application.Interfaces
{
    public interface IShipmentDocumentService
    {
        Task<IEnumerable<ShipmentDocument>> GetAllAsync();
        Task<ShipmentDocument?> GetByIdAsync(int id);
        Task<int> CreateAsync(ShipmentDocument document);
        Task<bool> UpdateAsync(int id, ShipmentDocument document);
        Task<bool> DeleteAsync(int id);

        Task<bool> SignDocumentAsync(int id);
        Task<bool> RevokeDocumentAsync(int id);

        Task<IEnumerable<ShipmentDocument>> GetFilteredAsync(ShipmentDocumentFilter filter);

        Task<bool> DeleteResourceFromDocumentAsync(int documentId, int resourceId);
    }
}
