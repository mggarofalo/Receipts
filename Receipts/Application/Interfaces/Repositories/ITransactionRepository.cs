using Domain.Core;

namespace Application.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
	Task<List<Transaction>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
}