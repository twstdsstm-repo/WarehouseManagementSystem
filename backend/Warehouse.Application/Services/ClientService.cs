using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

namespace Warehouse.Application.Services;

public class ClientService : IClientService
{
    private readonly WarehouseDbContext _context;

    public ClientService(WarehouseDbContext context)
    {
        _context = context;
    }

    
    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        return await _context.Clients.ToListAsync();
    }

    
    public async Task<Client?> GetByIdAsync(int id)
    {
        return await _context.Clients.FindAsync(id);
    }

    
    public async Task<Client> CreateAsync(Client client)
    {
        if (_context.Clients.Any(c => c.Name == client.Name))
            throw new InvalidOperationException("Клиент с таким именем уже существует.");

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client; 
    }

    
    public async Task<Client?> UpdateAsync(int id, Client updatedClient)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return null;

        client.Name = updatedClient.Name;
        client.Address = updatedClient.Address;
        client.IsArchived = updatedClient.IsArchived;

        await _context.SaveChangesAsync();
        return client;
    }
    
    public async Task<bool> ArchiveAsync(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
            return false;
        
        bool inUse = await _context.ShipmentDocuments.AnyAsync(d => d.ClientId == id);
        if (inUse)
        {
            
            client.IsArchived = true;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }
        else
        {
            client.IsArchived = true; 
            _context.Clients.Update(client); 
            await _context.SaveChangesAsync(); 
        }

        return true;
    }

    
    public async Task UnarchiveAsync(int id)
    {
        var entity = await _context.Clients.FirstOrDefaultAsync(x => x.Id == id)
                     ?? throw new KeyNotFoundException("Client not found");

        if (entity.IsArchived)
        {
            entity.IsArchived = false;
            await _context.SaveChangesAsync();
        }
    }

    
    public async Task<bool> DeleteAsync(int id)
    {
        
        var inUse = await _context.ShipmentDocuments.AnyAsync(d => d.ClientId == id);

        if (inUse) throw new InvalidOperationException("Клиент используется в других документах");

        var entity = await _context.Clients.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return false;

        _context.Clients.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
