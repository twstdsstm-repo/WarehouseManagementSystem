using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;
using Warehouse.Application.Models.Filters;

namespace Warehouse.Application.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly WarehouseDbContext _context;

        public BalanceService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<List<Balance>> GetAllAsync()
        {
            return await _context.Balances.ToListAsync();
        }

        public async Task<Balance> CreateAsync(Balance balance)
        {
            _context.Balances.Add(balance);
            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task<Balance?> UpdateAsync(int id, Balance updated)
        {
            var existing = await _context.Balances.FindAsync(id);
            if (existing == null) return null;

            existing.ResourceId = updated.ResourceId;
            existing.UnitOfMeasurementId = updated.UnitOfMeasurementId;
            existing.Quantity = updated.Quantity;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Balance?> GetByIdAsync(int id)
        {
            return await _context.Balances
                .Include(b => b.Resource)
                .Include(b => b.UnitOfMeasurement)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Balance>> GetFilteredAsync(BalanceFilter filter)
        {
            var query = _context.Balances
                .Include(b => b.Resource)
                .Include(b => b.UnitOfMeasurement)
                .AsQueryable();
            if (filter.ResourceIds != null && filter.ResourceIds.Any())
                query = query.Where(b => filter.ResourceIds.Contains(b.ResourceId));
            if (filter.UnitOfMeasurementIds != null && filter.UnitOfMeasurementIds.Any())
                query = query.Where(b => filter.UnitOfMeasurementIds.Contains(b.UnitOfMeasurementId));

            return await query.ToListAsync();
        }

        public async Task<Balance?> GetByResourceIdAsync(int resourceId, int unitOfMeasurementId)
        {
            return await _context.Balances
                .Include(b => b.Resource)
                .Include(b => b.UnitOfMeasurement)
                .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitOfMeasurementId == unitOfMeasurementId);
        }

        
        public async Task<bool> DeleteAsync(int id)
        {
            var balance = await _context.Balances.FindAsync(id);
            if (balance == null) return false;

            var isUsedInReceipt = await _context.ReceiptDocuments
                .AnyAsync(d => d.ReceiptResources.Any(r => r.ResourceId == balance.ResourceId));

            var isUsedInShipment = await _context.ShipmentDocuments
                .AnyAsync(d => d.ShipmentResources.Any(r => r.ResourceId == balance.ResourceId));

            if (isUsedInReceipt || isUsedInShipment)
            {
                throw new InvalidOperationException("Используется в других документах");
                
            }
            _context.Balances.Remove(balance);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
