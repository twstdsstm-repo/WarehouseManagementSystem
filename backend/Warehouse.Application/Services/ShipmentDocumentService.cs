using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Models.Filters;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

namespace Warehouse.Application.Services
{
    public class ShipmentDocumentService : IShipmentDocumentService
    {
        private readonly WarehouseDbContext _context;

        public ShipmentDocumentService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShipmentDocument>> GetAllAsync()
        {
            return await _context.ShipmentDocuments
                .Include(x => x.ShipmentResources)
                .ToListAsync();
        }

        public async Task<ShipmentDocument?> GetByIdAsync(int id)
        {
            return await _context.ShipmentDocuments
                .Include(x => x.ShipmentResources)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> CreateAsync(ShipmentDocument document)
        {
            if (await _context.ShipmentDocuments.AnyAsync(d => d.Number == document.Number))
                throw new InvalidOperationException("Документ с таким номером уже существует.");

            var client = await _context.Clients.FindAsync(document.ClientId);
            if (client == null || client.IsArchived)
                throw new InvalidOperationException("Нельзя использовать архивного клиента.");

            if (document.ShipmentResources == null || !document.ShipmentResources.Any())
                throw new InvalidOperationException("Документ отгрузки не может быть пустым.");
            if (document.ShipmentResources.Any(r => r.Quantity <= 0))
                throw new InvalidOperationException("Количество в позициях должно быть больше 0.");

            foreach (var res in document.ShipmentResources)
            {
                bool isResArchived = await _context.Resources
                    .Where(r => r.Id == res.ResourceId)
                    .Select(r => r.IsArchived)
                    .FirstOrDefaultAsync();

                bool isUnitArchived = await _context.UnitsOfMeasurement
                    .Where(u => u.Id == res.UnitOfMeasurementId)
                    .Select(u => u.IsArchived)
                    .FirstOrDefaultAsync();

                if (isResArchived || isUnitArchived)
                    throw new InvalidOperationException("Архивные ресурсы или единицы измерения недоступны.");

                var balance = await _context.Balances
                    .FirstOrDefaultAsync(b => b.ResourceId == res.ResourceId &&
                                              b.UnitOfMeasurementId == res.UnitOfMeasurementId);
            }

            document.State = false; 

            using var tx = await _context.Database.BeginTransactionAsync();
            _context.ShipmentDocuments.Add(document);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return document.Id;
        }

        public async Task<bool> UpdateAsync(int id, ShipmentDocument updatedDoc)
        {
            var existingDoc = await _context.ShipmentDocuments
                .Include(x => x.ShipmentResources)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingDoc == null)
                return false;

            if (existingDoc.State)
                throw new InvalidOperationException("Нельзя редактировать подписанный документ.");

            if (updatedDoc.ShipmentResources == null || !updatedDoc.ShipmentResources.Any())
                throw new InvalidOperationException("Документ отгрузки не может быть пустым.");
            
            if (updatedDoc.ShipmentResources.Any(r => r.Quantity <= 0))
                throw new InvalidOperationException("Количество в позициях должно быть больше 0.");

            
            var client = await _context.Clients.FindAsync(updatedDoc.ClientId);
            if (client == null)
                throw new InvalidOperationException("Клиент не найден.");
            if (client.IsArchived)
                throw new InvalidOperationException("Нельзя использовать архивного клиента.");

            
            foreach (var res in updatedDoc.ShipmentResources)
            {
                var resource = await _context.Resources.FindAsync(res.ResourceId);
                var unit = await _context.UnitsOfMeasurement.FindAsync(res.UnitOfMeasurementId);
                
                if (resource == null || unit == null)
                    throw new InvalidOperationException("Ресурс или единица измерения не найдены.");
                    
                if (resource.IsArchived || unit.IsArchived)
                    throw new InvalidOperationException("Нельзя использовать архивный ресурс или единицу измерения.");
            }

            existingDoc.Number = updatedDoc.Number;
            existingDoc.Date = updatedDoc.Date;
            existingDoc.ClientId = updatedDoc.ClientId;

            
            var existingResources = existingDoc.ShipmentResources.ToList();
            foreach (var updatedRes in updatedDoc.ShipmentResources)
            {
                var existingRes = existingResources.FirstOrDefault(r => r.Id == updatedRes.Id);
                if (existingRes != null)
                {
                    existingRes.ResourceId = updatedRes.ResourceId;
                    existingRes.UnitOfMeasurementId = updatedRes.UnitOfMeasurementId;
                    existingRes.Quantity = updatedRes.Quantity;
                }
                else
                {
                    existingDoc.ShipmentResources.Add(updatedRes);
                }
            }

            var updatedIds = updatedDoc.ShipmentResources.Select(r => r.Id).ToHashSet();
            var toRemove = existingResources.Where(r => !updatedIds.Contains(r.Id)).ToList();
            _context.ShipmentResources.RemoveRange(toRemove);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingDoc = await _context.ShipmentDocuments
                .Include(x => x.ShipmentResources)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingDoc == null)
                return false;

            if (existingDoc.State)
                throw new InvalidOperationException("Нельзя удалить подписанный документ.");

            _context.ShipmentResources.RemoveRange(existingDoc.ShipmentResources);
            _context.ShipmentDocuments.Remove(existingDoc);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SignDocumentAsync(int id)
        {
            var doc = await _context.ShipmentDocuments
                .Include(x => x.ShipmentResources)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doc == null)
                return false;

            if (doc.State)
                throw new InvalidOperationException("Документ уже подписан.");

            if (doc.ShipmentResources == null || !doc.ShipmentResources.Any())
                throw new InvalidOperationException("Документ отгрузки не может быть пустым.");
            if (doc.ShipmentResources.Any(r => r.Quantity <= 0))
                throw new InvalidOperationException("Количество в позициях должно быть больше 0.");

            using var tx = await _context.Database.BeginTransactionAsync();

            foreach (var res in doc.ShipmentResources)
            {
                var balance = await _context.Balances
                    .FirstOrDefaultAsync(b => b.ResourceId == res.ResourceId &&
                                              b.UnitOfMeasurementId == res.UnitOfMeasurementId);

                if (balance == null || balance.Quantity < res.Quantity)
                    throw new InvalidOperationException("Недостаточно ресурсов для подписания.");
            }

            foreach (var res in doc.ShipmentResources)
            {
                var balance = await _context.Balances
                    .FirstOrDefaultAsync(b => b.ResourceId == res.ResourceId &&
                                              b.UnitOfMeasurementId == res.UnitOfMeasurementId);

                if (balance != null)
                    balance.Quantity -= res.Quantity;
            }

            doc.State = true;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }

        public async Task<bool> RevokeDocumentAsync(int id)
        {
            var doc = await _context.ShipmentDocuments
                .Include(x => x.ShipmentResources)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doc == null)
                return false;

            if (!doc.State)
                throw new InvalidOperationException("Документ не подписан.");

            using var tx = await _context.Database.BeginTransactionAsync();

            foreach (var res in doc.ShipmentResources)
            {
                var balance = await _context.Balances
                    .FirstOrDefaultAsync(b => b.ResourceId == res.ResourceId &&
                                              b.UnitOfMeasurementId == res.UnitOfMeasurementId);

                if (balance != null)
                    balance.Quantity += res.Quantity;
            }

            doc.State = false;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<ShipmentDocument>> GetFilteredAsync(ShipmentDocumentFilter filter)
        {
            var query = _context.ShipmentDocuments
                .Include(x => x.ShipmentResources)
                .AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(x => x.Date >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.Date <= filter.ToDate.Value);

            if (filter.Numbers != null && filter.Numbers.Any())
                query = query.Where(x => filter.Numbers.Contains(x.Number));

            if (filter.ResourceIds != null && filter.ResourceIds.Any())
                query = query.Where(x => x.ShipmentResources.Any(r => filter.ResourceIds.Contains(r.ResourceId)));

            if (filter.UnitOfMeasurementIds != null && filter.UnitOfMeasurementIds.Any())
                query = query.Where(x => x.ShipmentResources.Any(r => filter.UnitOfMeasurementIds.Contains(r.UnitOfMeasurementId)));

            return await query.ToListAsync();
        }

        public async Task<bool> DeleteResourceFromDocumentAsync(int documentId, int resourceId)
        {
            var document = await _context.ShipmentDocuments
                .Include(d => d.ShipmentResources)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null) return false;

            var resourceToDelete = document.ShipmentResources.FirstOrDefault(r => r.ResourceId == resourceId);
            if (resourceToDelete == null) return false;

            
            document.ShipmentResources.Remove(resourceToDelete);

            
            if (!document.ShipmentResources.Any())
            {
                _context.ShipmentDocuments.Remove(document);
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
