using Warehouse.Domain.Entities;

namespace Warehouse.Application.Interfaces;

public interface IClientService
{
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(int id);
    Task<Client> CreateAsync(Client client);
    Task<Client?> UpdateAsync(int id, Client client);
    Task<bool> ArchiveAsync(int id);

    Task UnarchiveAsync(int id);   
    Task<bool> DeleteAsync(int id); 
}