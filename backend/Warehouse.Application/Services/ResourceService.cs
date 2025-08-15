using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

namespace Warehouse.Application.Services
{
    public class ResourceService : IResourceService
    {
        private readonly WarehouseDbContext _context;

        public ResourceService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Resource>> GetAllAsync()
        {
            return await _context.Resources.ToListAsync();
        }

        public async Task<Resource?> GetByIdAsync(int id)
        {
            return await _context.Resources.FindAsync(id);
        }

        public async Task<Resource> CreateAsync(Resource resource)
        {
            if (await _context.Resources.AnyAsync(r => r.Name == resource.Name && !r.IsArchived))
                throw new InvalidOperationException("Ресурс с таким наименованием уже существует.");

            
            if (string.IsNullOrEmpty(resource.Name))
            {
                throw new InvalidOperationException("Имя обязательно");
            }

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            return resource;
        }

        public async Task<bool> UpdateAsync(int id, Resource updatedResource)
        {
            var existing = await _context.Resources.FindAsync(id);
            if (existing == null) return false;

            existing.Name = updatedResource.Name;
            existing.IsArchived = updatedResource.IsArchived;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveAsync(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return false;

            bool inUse = await _context.Balances.AnyAsync(b => b.ResourceId == id)
                      || await _context.ReceiptResources.AnyAsync(r => r.ResourceId == id)
                      || await _context.ShipmentResources.AnyAsync(s => s.ResourceId == id);

            if (inUse)
            {
                resource.IsArchived = true;
                _context.Resources.Update(resource);
                await _context.SaveChangesAsync();
            }
            else
            {
                resource.IsArchived = true; 
                _context.Resources.Update(resource); 
                await _context.SaveChangesAsync(); 
            }

            return true;
        }

        public async Task UnarchiveAsync(int id)
        {
            var entity = await _context.Resources.FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new KeyNotFoundException("Resource not found");

            if (entity.IsArchived)
            {
                entity.IsArchived = false;
                await _context.SaveChangesAsync();
            }
        }

            public async Task<bool> DeleteAsync(int id)
            {
                
                bool isUsedInReceipt = await _context.ReceiptResources.AnyAsync(r => r.ResourceId == id);
                bool isUsedInShipment = await _context.ShipmentResources.AnyAsync(s => s.ResourceId == id);

                if (isUsedInReceipt || isUsedInShipment)
                {
                    throw new InvalidOperationException("Ресурс не может быть удален, так как он используется в других документах.");
                }

                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id);
                if (resource == null)
                {
                    return false;
                }

                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
                return true;
            }
    }
}
