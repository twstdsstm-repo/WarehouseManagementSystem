using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

namespace Warehouse.Application.Services;

public class ShipmentResourceService : IShipmentResourceService
{
    private readonly WarehouseDbContext _context;

    public ShipmentResourceService(WarehouseDbContext context)
    {
        _context = context;
    }


    
    public async Task<List<ShipmentResource>> GetAllAsync()
    {
        return await _context.ShipmentResources.ToListAsync();
    }

    public async Task<ShipmentResource?> GetByIdAsync(int id)
    {
        return await _context.ShipmentResources
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<int> CreateAsync(ShipmentResource resource)
    {
        if (await IsArchived(resource.ResourceId, resource.UnitOfMeasurementId))
            throw new InvalidOperationException("Нельзя использовать архивный ресурс или единицу измерения.");

        bool ok = await UpdateBalance(resource.ResourceId, resource.UnitOfMeasurementId, -(decimal)resource.Quantity);
        if (!ok) throw new InvalidOperationException("Недостаточно ресурсов на складе.");

        _context.ShipmentResources.Add(resource);
        await _context.SaveChangesAsync();
        return resource.Id;
    }

    public async Task<bool> UpdateAsync(int id, ShipmentResource updated)
    {
        var existing = await _context.ShipmentResources.FindAsync(id);
        if (existing == null) return false;

        bool rollback = await UpdateBalance(existing.ResourceId, existing.UnitOfMeasurementId, (decimal)existing.Quantity);
        if (!rollback) throw new InvalidOperationException("Ошибка при возврате старого значения.");

        if (await IsArchived(updated.ResourceId, updated.UnitOfMeasurementId))
            throw new InvalidOperationException("Архивный ресурс или единица измерения.");

        bool apply = await UpdateBalance(updated.ResourceId, updated.UnitOfMeasurementId, -(decimal)updated.Quantity);
        if (!apply) throw new InvalidOperationException("Недостаточно ресурсов на складе.");

        existing.ResourceId = updated.ResourceId;
        existing.UnitOfMeasurementId = updated.UnitOfMeasurementId;
        existing.Quantity = updated.Quantity;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.ShipmentResources.FindAsync(id);
        if (existing == null) return false;

        await UpdateBalance(existing.ResourceId, existing.UnitOfMeasurementId, (decimal)existing.Quantity);

        _context.ShipmentResources.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateBalance(int resourceId, int unitId, decimal delta)
    {
        var balance = await _context.Balances
            .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitOfMeasurementId == unitId);

        if (balance == null || balance.Quantity + delta < 0)
            return false;

        balance.Quantity += delta;
        return true;
    }

    public async Task<bool> IsArchived(int resourceId, int unitId)
    {
        var res = await _context.Resources
            .Where(r => r.Id == resourceId)
            .Select(r => r.IsArchived)
            .FirstOrDefaultAsync();

        var unit = await _context.UnitsOfMeasurement
            .Where(u => u.Id == unitId)
            .Select(u => u.IsArchived)
            .FirstOrDefaultAsync();

        return res || unit;
    }
}

