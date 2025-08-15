using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

namespace Warehouse.Application.Services
{
    public class UnitOfMeasurementService : IUnitOfMeasurementService
    {
        private readonly WarehouseDbContext _context;

        public UnitOfMeasurementService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UnitOfMeasurement>> GetAllAsync()
        {
            return await _context.UnitsOfMeasurement.ToListAsync();
        }

        public async Task<UnitOfMeasurement?> GetByIdAsync(int id)
        {
            return await _context.UnitsOfMeasurement.FindAsync(id);
        }

        public async Task<int> CreateAsync(UnitOfMeasurement unit)
        {
            if (await _context.UnitsOfMeasurement.AnyAsync(u => u.Name == unit.Name))
                throw new InvalidOperationException("Единица измерения с таким наименованием уже существует.");

            _context.UnitsOfMeasurement.Add(unit);
            await _context.SaveChangesAsync();
            return unit.Id;
        }

        public async Task<bool> UpdateAsync(int id, UnitOfMeasurement unit)
        {
            var existing = await _context.UnitsOfMeasurement.FindAsync(id);
            if (existing == null) return false;

            existing.Name = unit.Name;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveAsync(int id)
        {
            var unit = await _context.UnitsOfMeasurement.FindAsync(id);
            if (unit == null)
                return false;

            
            bool inUse = await _context.Balances.AnyAsync(b => b.UnitOfMeasurementId == id)
                      || await _context.ReceiptResources.AnyAsync(r => r.UnitOfMeasurementId == id)
                      || await _context.ShipmentResources.AnyAsync(s => s.UnitOfMeasurementId == id);

            if (inUse)
            {
                unit.IsArchived = true;
                _context.UnitsOfMeasurement.Update(unit);
                await _context.SaveChangesAsync();
            }
            else
            {
                unit.IsArchived = true; 
                _context.UnitsOfMeasurement.Update(unit); 
                await _context.SaveChangesAsync(); 
            }

            return true;
        }

        public async Task UnarchiveAsync(int id)
        {
            var entity = await _context.UnitsOfMeasurement.FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new KeyNotFoundException("Unit of measurement not found");

            if (entity.IsArchived)
            {
                entity.IsArchived = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            
            var inUse =
                await _context.ReceiptResources.AnyAsync(r => r.UnitOfMeasurementId == id) ||
                await _context.ShipmentResources.AnyAsync(r => r.UnitOfMeasurementId == id) ||
                await _context.Balances.AnyAsync(b => b.UnitOfMeasurementId == id);

            if (inUse) throw new InvalidOperationException("Используется в других документах");

            var entity = await _context.UnitsOfMeasurement.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return false;

            _context.UnitsOfMeasurement.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
