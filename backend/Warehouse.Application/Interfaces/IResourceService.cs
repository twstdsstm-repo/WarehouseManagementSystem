using Warehouse.Domain.Entities;

namespace Warehouse.Application.Interfaces
{
    public interface IResourceService
    {
        Task<IEnumerable<Resource>> GetAllAsync();
        Task<Resource?> GetByIdAsync(int id);
        Task<Resource> CreateAsync(Resource resource);
        Task<bool> UpdateAsync(int id, Resource updatedResource);
        Task<bool> ArchiveAsync(int id);
        Task UnarchiveAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
