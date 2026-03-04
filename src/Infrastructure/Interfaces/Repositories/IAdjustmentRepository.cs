using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface IAdjustmentRepository
{
	Task<AdjustmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<AdjustmentEntity>> GetByReceiptIdAsync(Guid receiptId, int offset, int limit, CancellationToken cancellationToken);
	Task<int> GetByReceiptIdCountAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<AdjustmentEntity>> GetAllAsync(int offset, int limit, CancellationToken cancellationToken);
	Task<List<AdjustmentEntity>> GetDeletedAsync(int offset, int limit, CancellationToken cancellationToken);
	Task<int> GetDeletedCountAsync(CancellationToken cancellationToken);
	Task<List<AdjustmentEntity>> CreateAsync(List<AdjustmentEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<AdjustmentEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
}
