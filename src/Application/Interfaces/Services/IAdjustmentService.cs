using Application.Models;
using Domain.Core;

namespace Application.Interfaces.Services;

public interface IAdjustmentService : ISoftDeletableService<Adjustment>
{
	Task<PagedResult<Adjustment>> GetByReceiptIdAsync(Guid receiptId, int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<List<Adjustment>> CreateAsync(List<Adjustment> models, Guid receiptId, CancellationToken cancellationToken);
	Task UpdateAsync(List<Adjustment> models, Guid receiptId, CancellationToken cancellationToken);
}
