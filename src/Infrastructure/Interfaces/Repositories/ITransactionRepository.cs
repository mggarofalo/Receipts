using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface ITransactionRepository
{
	Task<TransactionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<TransactionEntity>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<TransactionEntity>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<TransactionEntity>> CreateAsync(List<TransactionEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<TransactionEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
}
