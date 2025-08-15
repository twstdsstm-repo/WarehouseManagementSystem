using Warehouse.Domain.Entities;

namespace Warehouse.Application.Interfaces
{
    public interface IReceiptResourceService
    {
        Task<List<ReceiptResource>> GetAllAsync();
        Task<ReceiptResource?> GetByIdAsync(int id);
        Task<int> CreateAsync(ReceiptResource entity);
        Task<bool> UpdateAsync(int id, ReceiptResource entity);
        Task<bool> DeleteAsync(int id);
        Task<IReadOnlyList<ReceiptResource>> GetByDocumentIdAsync(int receiptDocumentId);  // Метод для получения ресурсов по документу
    }
}
