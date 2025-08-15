using Warehouse.Domain.Entities;
using Warehouse.Application.Models.Filters;

namespace Warehouse.Application.Interfaces
{
    public interface IReceiptDocumentService
    {
        Task<List<ReceiptDocument>> GetAllAsync();
        Task<ReceiptDocument?> GetByIdAsync(int id);
        Task<int> CreateAsync(ReceiptDocument document);
        Task<bool> UpdateAsync(int id, ReceiptDocument updatedDocument);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateBalance(int resourceId, int unitId, decimal delta);
        Task<IEnumerable<ReceiptDocument>> GetFilteredAsync(ReceiptDocumentFilter filter);
        Task<IReadOnlyList<ReceiptResource>> GetByDocumentIdAsync(int receiptDocumentId);
        Task<bool> DeleteResourceFromDocumentAsync(int documentId, int resourceId);
    }
}
