using Application.Models;
using Domain.Core;

namespace Application.Interfaces.Services;

public interface ITransactionService : IService<Transaction>
{
	Task<PagedResult<Transaction>> GetByReceiptIdAsync(Guid receiptId, int offset, int limit, CancellationToken cancellationToken);
	Task<List<Domain.Aggregates.TransactionAccount>> GetTransactionAccountsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<Transaction>> CreateAsync(List<Transaction> models, Guid receiptId, CancellationToken cancellationToken);
	Task UpdateAsync(List<Transaction> models, Guid receiptId, CancellationToken cancellationToken);
}