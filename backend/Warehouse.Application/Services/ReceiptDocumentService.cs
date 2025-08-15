using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Application.Models.Filters;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

namespace Warehouse.Application.Services
{
    public class ReceiptDocumentService : IReceiptDocumentService
    {
        private readonly WarehouseDbContext _context;
        public ReceiptDocumentService(WarehouseDbContext context)
        {
            _context = context;
        }
        public async Task<List<ReceiptDocument>> GetAllAsync()
        {
            return await _context.ReceiptDocuments
                .Include(d => d.ReceiptResources)
                .ToListAsync();
        }
        public async Task<IReadOnlyList<ReceiptResource>> GetByDocumentIdAsync(int receiptDocumentId)
        {
            return await _context.ReceiptResources
                .AsNoTracking()
                .Where(r => r.ReceiptDocumentId == receiptDocumentId)
                .Include(r => r.Resource)
                .Include(r => r.UnitOfMeasurement)
                .ToListAsync();
        }

        public async Task<ReceiptDocument?> GetByIdAsync(int id)
        {
            return await _context.ReceiptDocuments
                .Include(d => d.ReceiptResources)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<int> CreateAsync(ReceiptDocument document)
        {
            if (await _context.ReceiptDocuments.AnyAsync(d => d.Number == document.Number))
                throw new InvalidOperationException("Документ с таким номером уже существует.");

            if (document.ReceiptResources.Any(r => r.Quantity < 0))
               throw new InvalidOperationException("Количество не может быть отрицательным.");

            using var tx = await _context.Database.BeginTransactionAsync();

            
            var validResources = document.ReceiptResources?
                .Where(r => r.Quantity > 0)
                .ToList() ?? new List<ReceiptResource>();

            foreach (var resource in validResources)
            {
                if (await IsArchived(resource.ResourceId, resource.UnitOfMeasurementId))
                    throw new InvalidOperationException("Архивный ресурс или единица измерения.");

                resource.ReceiptDocument = document;
                await UpdateBalance(resource.ResourceId, resource.UnitOfMeasurementId, resource.Quantity);
            }
            document.ReceiptResources = validResources;
            _context.ReceiptDocuments.Add(document);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return document.Id;
        }

        public async Task<bool> UpdateAsync(int id, ReceiptDocument updated)
        {
            var existing = await _context.ReceiptDocuments
                .Include(d => d.ReceiptResources)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existing == null) return false;

            if (updated.ReceiptResources.Any(r => r.Quantity <= 0))
                throw new InvalidOperationException("Количество должно быть больше 0.");

            using var tx = await _context.Database.BeginTransactionAsync();
            
            foreach (var res in updated.ReceiptResources)
            {
                var resource = await _context.Resources.FindAsync(res.ResourceId);
                var unit = await _context.UnitsOfMeasurement.FindAsync(res.UnitOfMeasurementId);
                
                if (resource == null || unit == null)
                    throw new InvalidOperationException("Ресурс или единица измерения не найдены.");
                    
                if (resource.IsArchived || unit.IsArchived)
                    throw new InvalidOperationException("Нельзя использовать архивный ресурс или единицу измерения.");
            }
            
            foreach (var old in existing.ReceiptResources)
            {
                bool ok = await UpdateBalance(old.ResourceId, old.UnitOfMeasurementId, -old.Quantity);
                if (!ok) throw new InvalidOperationException("Недостаточно ресурсов для редактирования (откат старых).");
            }
            
            foreach (var res in updated.ReceiptResources)
            {
                await UpdateBalance(res.ResourceId, res.UnitOfMeasurementId, res.Quantity);
            }

            existing.Number = updated.Number;
            existing.Date = updated.Date;

            var idsToKeep = updated.ReceiptResources.Select(r => r.Id).ToList();
            var toRemove = existing.ReceiptResources.Where(r => !idsToKeep.Contains(r.Id)).ToList();
            _context.ReceiptResources.RemoveRange(toRemove);

            foreach (var updatedRes in updated.ReceiptResources)
            {
                var existingRes = existing.ReceiptResources.FirstOrDefault(r => r.Id == updatedRes.Id);
                if (existingRes != null)
                {
                    existingRes.ResourceId = updatedRes.ResourceId;
                    existingRes.UnitOfMeasurementId = updatedRes.UnitOfMeasurementId;
                    existingRes.Quantity = updatedRes.Quantity;
                }
                else
                {
                    existing.ReceiptResources.Add(updatedRes);
                }
            }
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _context.ReceiptDocuments
                .Include(d => d.ReceiptResources)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (document == null) return false;
            using var tx = await _context.Database.BeginTransactionAsync();
            foreach (var res in document.ReceiptResources)
            {
                bool ok = await UpdateBalance(res.ResourceId, res.UnitOfMeasurementId, -res.Quantity);
                if (!ok) throw new InvalidOperationException("Недостаточно ресурсов для удаления.");
            }
            _context.ReceiptDocuments.Remove(document);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateBalance(int resourceId, int unitId, decimal delta)
        {
            var balance = await _context.Balances
                .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitOfMeasurementId == unitId);

            if (balance == null)
            {
                if (delta < 0) return false;
                _context.Balances.Add(new Balance
                {
                    ResourceId = resourceId,
                    UnitOfMeasurementId = unitId,
                    Quantity = delta
                });
            }
            else
            {
                if (balance.Quantity + delta < 0) return false;
                balance.Quantity += delta;
            }

            return true;
        }

        private async Task<bool> IsArchived(int resourceId, int unitId)
        {
            var resourceArchived = await _context.Resources
                .Where(r => r.Id == resourceId)
                .Select(r => r.IsArchived)
                .FirstOrDefaultAsync();
            var unitArchived = await _context.UnitsOfMeasurement
                .Where(u => u.Id == unitId)
                .Select(u => u.IsArchived)
                .FirstOrDefaultAsync();

            return resourceArchived || unitArchived;
        }

        public async Task<IEnumerable<ReceiptDocument>> GetFilteredAsync(ReceiptDocumentFilter filter)
        {
            var query = _context.ReceiptDocuments
                .Include(x => x.ReceiptResources)
                .AsQueryable();
            if (filter.FromDate.HasValue)
                query = query.Where(x => x.Date >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(x => x.Date <= filter.ToDate.Value);
            if (filter.Numbers != null && filter.Numbers.Any())
                query = query.Where(x => filter.Numbers.Contains(x.Number));
            if (filter.ResourceIds != null && filter.ResourceIds.Any())
                query = query.Where(x => x.ReceiptResources.Any(r => filter.ResourceIds.Contains(r.ResourceId)));
            if (filter.UnitOfMeasurementIds != null && filter.UnitOfMeasurementIds.Any())
                query = query.Where(x => x.ReceiptResources.Any(r => filter.UnitOfMeasurementIds.Contains(r.UnitOfMeasurementId)));
            return await query.ToListAsync();
        }

        public async Task<bool> DeleteResourceFromDocumentAsync(int documentId, int resourceId)
        {
            var document = await _context.ReceiptDocuments
                .Include(d => d.ReceiptResources)
                .FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null) return false;  
            var resourceToDelete = document.ReceiptResources.FirstOrDefault(r => r.ResourceId == resourceId);
            if (resourceToDelete == null) return false;  
            var balance = await _context.Balances
                .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitOfMeasurementId == resourceToDelete.UnitOfMeasurementId);
            if (balance != null)
            {
                balance.Quantity -= resourceToDelete.Quantity;
                if (balance.Quantity < 0)
                {
                    throw new InvalidOperationException("Недостаточно ресурсов для выполнения операции.");
                }

                _context.Balances.Update(balance);
            }
            document.ReceiptResources.Remove(resourceToDelete);
            if (document.ReceiptResources.Count == 0)
            {
                _context.ReceiptDocuments.Remove(document);  
            }

            await _context.SaveChangesAsync();  

            return true;  
        }


    }
}
