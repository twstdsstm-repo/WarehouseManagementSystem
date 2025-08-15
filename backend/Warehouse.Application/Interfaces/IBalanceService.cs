using Warehouse.Domain.Entities;
using Warehouse.Application.Models.Filters;

namespace Warehouse.Application.Interfaces
{
    public interface IBalanceService
    {
        Task<List<Balance>> GetAllAsync();
        Task<Balance?> GetByIdAsync(int id);
        Task<Balance> CreateAsync(Balance balance);
        Task<Balance?> UpdateAsync(int id, Balance balance);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Balance>> GetFilteredAsync(BalanceFilter filter);
        Task<Balance?> GetByResourceIdAsync(int resourceId, int unitOfMeasurementId);
    }
}
