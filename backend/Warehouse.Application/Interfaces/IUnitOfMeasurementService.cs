using Warehouse.Domain.Entities;

namespace Warehouse.Application.Interfaces
{
    public interface IUnitOfMeasurementService
    {
        Task<IEnumerable<UnitOfMeasurement>> GetAllAsync();
        Task<UnitOfMeasurement?> GetByIdAsync(int id);
        Task<int> CreateAsync(UnitOfMeasurement unit);
        Task<bool> UpdateAsync(int id, UnitOfMeasurement unit);
        Task<bool> ArchiveAsync(int id);
        Task UnarchiveAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
