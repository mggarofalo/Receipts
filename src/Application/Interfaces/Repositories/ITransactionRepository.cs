using Application.Commands.Transaction;
using Domain.Core;

namespace Application.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
	Task<List<Transaction>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<Transaction>> CreateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken);
	Task UpdateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken);
}