using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

namespace Warehouse.Application.Services
{
    public class ReceiptResourceService : IReceiptResourceService
    {
        private readonly WarehouseDbContext _context;

        public ReceiptResourceService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReceiptResource>> GetAllAsync()
        {
            return await _context.ReceiptResources.ToListAsync();
        }

        public async Task<ReceiptResource?> GetByIdAsync(int id)
        {
            return await _context.ReceiptResources.FindAsync(id);
        }

        public async Task<int> CreateAsync(ReceiptResource resource)
        {
            var isResourceArchived = await _context.Resources
                .Where(r => r.Id == resource.ResourceId)
                .Select(r => r.IsArchived)
                .FirstOrDefaultAsync();

            var isUnitArchived = await _context.UnitsOfMeasurement
                .Where(u => u.Id == resource.UnitOfMeasurementId)
                .Select(u => u.IsArchived)
                .FirstOrDefaultAsync();

            if (isResourceArchived || isUnitArchived)
                throw new InvalidOperationException("Нельзя использовать архивный ресурс или единицу измерения.");

            _context.ReceiptResources.Add(resource);

            await UpdateBalance(resource.ResourceId, resource.UnitOfMeasurementId, (decimal)resource.Quantity);

            await _context.SaveChangesAsync();
            return resource.Id;
        }

        public async Task<bool> UpdateAsync(int id, ReceiptResource updated)
        {
            var existing = await _context.ReceiptResources.FindAsync(id);
            if (existing == null) return false;

            bool canRollback = await UpdateBalance(existing.ResourceId, existing.UnitOfMeasurementId, -((decimal)existing.Quantity));
            if (!canRollback)
                throw new InvalidOperationException("Недостаточно ресурсов для редактирования (возврат старого значения).");

            var isResourceArchived = await _context.Resources
                .Where(r => r.Id == updated.ResourceId)
                .Select(r => r.IsArchived)
                .FirstOrDefaultAsync();

            var isUnitArchived = await _context.UnitsOfMeasurement
                .Where(u => u.Id == updated.UnitOfMeasurementId)
                .Select(u => u.IsArchived)
                .FirstOrDefaultAsync();

            if ((isResourceArchived || isUnitArchived) &&
                (existing.ResourceId != updated.ResourceId || existing.UnitOfMeasurementId != updated.UnitOfMeasurementId))
            {
                throw new InvalidOperationException("Архивный ресурс или единицу нельзя выбрать.");
            }

            await UpdateBalance(updated.ResourceId, updated.UnitOfMeasurementId, (decimal)updated.Quantity);

            existing.ResourceId = updated.ResourceId;
            existing.UnitOfMeasurementId = updated.UnitOfMeasurementId;
            existing.Quantity = updated.Quantity;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resource = await _context.ReceiptResources.FindAsync(id);
            if (resource == null) return false;

            bool canRollback = await UpdateBalance(resource.ResourceId, resource.UnitOfMeasurementId, -((decimal)resource.Quantity));
            if (!canRollback)
                throw new InvalidOperationException("Недостаточно ресурсов для удаления.");

            _context.ReceiptResources.Remove(resource);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBalance(int resourceId, int unitId, decimal delta)
        {
            var balance = await _context.Balances
                .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitOfMeasurementId == unitId);

            if (balance == null)
            {
                if (delta < 0) return false;

                var resource = await _context.Resources.FindAsync(resourceId);
                var unit = await _context.UnitsOfMeasurement.FindAsync(unitId);

                if (resource == null)
                    throw new InvalidOperationException($"Ресурс с id {resourceId} не найден");

                if (unit == null)
                    throw new InvalidOperationException($"Единица измерения с id {unitId} не найдена");

                balance = new Balance
                {
                    ResourceId = resourceId,
                    UnitOfMeasurementId = unitId,
                    Quantity = delta,
                    Resource = resource,
                    UnitOfMeasurement = unit
                };

                _context.Balances.Add(balance);
            }
            else
            {
                if (balance.Quantity + delta < 0) return false;
                balance.Quantity += delta;
            }

            return true;
        }

        // Реализация метода получения всех ресурсов для конкретного документа поступления
        public async Task<IReadOnlyList<ReceiptResource>> GetByDocumentIdAsync(int receiptDocumentId)
        {
            return await _context.ReceiptResources
                .Where(r => r.ReceiptDocumentId == receiptDocumentId)
                .Include(r => r.Resource)
                .Include(r => r.UnitOfMeasurement)
                .ToListAsync();
        }
    }
}
