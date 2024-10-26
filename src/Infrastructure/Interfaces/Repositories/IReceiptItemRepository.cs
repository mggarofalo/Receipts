using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface IReceiptItemRepository
{
	Task<ReceiptItemEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<ReceiptItemEntity>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<ReceiptItemEntity>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<ReceiptItemEntity>> CreateAsync(List<ReceiptItemEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<ReceiptItemEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
}
