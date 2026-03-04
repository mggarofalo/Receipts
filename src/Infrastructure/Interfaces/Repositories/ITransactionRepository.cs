using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface ITransactionRepository
{
	Task<TransactionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<TransactionEntity>> GetByReceiptIdAsync(Guid receiptId, int offset, int limit, CancellationToken cancellationToken);
	Task<List<TransactionEntity>> GetWithAccountByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<int> GetByReceiptIdCountAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<TransactionEntity>> GetAllAsync(int offset, int limit, CancellationToken cancellationToken);
	Task<List<TransactionEntity>> GetDeletedAsync(int offset, int limit, CancellationToken cancellationToken);
	Task<int> GetDeletedCountAsync(CancellationToken cancellationToken);
	Task<List<TransactionEntity>> CreateAsync(List<TransactionEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<TransactionEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
}
