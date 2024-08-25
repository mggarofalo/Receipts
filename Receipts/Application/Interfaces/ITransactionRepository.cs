using Domain;
using Domain.Core;

namespace Application.Interfaces;

public interface ITransactionRepository
{
	Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<Transaction>> GetByMoneyRangeAsync(Money minAmount, Money maxAmount, CancellationToken cancellationToken);
	Task<List<Transaction>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
	Task<List<Transaction>> CreateAsync(List<Transaction> transactions, CancellationToken cancellationToken);
	Task<bool> UpdateAsync(List<Transaction> transactions, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task SaveChangesAsync(CancellationToken cancellationToken);
}