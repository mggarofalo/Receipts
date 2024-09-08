using Domain.Core;

namespace Application.Interfaces.Repositories;

public interface IReceiptItemRepository : IRepository<ReceiptItem>
{
	Task<List<ReceiptItem>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<ReceiptItem>> CreateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken);
	Task UpdateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken);
}
