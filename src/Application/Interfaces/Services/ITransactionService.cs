using Domain.Core;

namespace Application.Interfaces.Services;

public interface ITransactionService : IService<Transaction>
{
	Task<List<Transaction>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<Transaction>> CreateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken);
	Task UpdateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken);
}