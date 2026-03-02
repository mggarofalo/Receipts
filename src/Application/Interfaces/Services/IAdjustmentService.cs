using Domain.Core;

namespace Application.Interfaces.Services;

public interface IAdjustmentService : IService<Adjustment>
{
	Task<List<Adjustment>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<Adjustment>> CreateAsync(List<Adjustment> models, Guid receiptId, CancellationToken cancellationToken);
	Task UpdateAsync(List<Adjustment> models, Guid receiptId, CancellationToken cancellationToken);
}
