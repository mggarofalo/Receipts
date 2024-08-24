using Domain;
using Domain.Core;

namespace Application.Interfaces;

public interface IReceiptRepository
{
	Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<Receipt>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<Receipt>> GetByLocationAsync(string location, CancellationToken cancellationToken);
	Task<List<Receipt>> GetByMoneyRangeAsync(Money minAmount, Money maxAmount, CancellationToken cancellationToken);
	Task<List<Receipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
	Task<Receipt> CreateAsync(Receipt receipt, CancellationToken cancellationToken);
	Task<bool> UpdateAsync(Receipt receipt, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task SaveChangesAsync(CancellationToken cancellationToken);
}
