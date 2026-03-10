using Application.Models;
using Domain.Core;

namespace Application.Interfaces.Services;

public interface IReceiptItemService : ISoftDeletableService<ReceiptItem>
{
	Task<PagedResult<ReceiptItem>> GetByReceiptIdAsync(Guid receiptId, int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<List<ReceiptItem>> CreateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken);
	Task UpdateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken);
}
