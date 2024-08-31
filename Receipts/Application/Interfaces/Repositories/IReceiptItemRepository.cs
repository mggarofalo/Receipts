using Domain.Core;

namespace Application.Interfaces.Repositories;

public interface IReceiptItemRepository : IRepository<ReceiptItem>
{
	Task<List<ReceiptItem>> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
}
