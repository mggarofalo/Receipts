using Domain.Core;

namespace Application.Interfaces;

public interface IAccountRepository
{
	Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<Account>> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken);
	Task<List<Account>> GetByNameAsync(string name, CancellationToken cancellationToken);
	Task<List<Account>> GetAllAsync(CancellationToken cancellationToken);
	Task<Account> CreateAsync(Account account, CancellationToken cancellationToken);
	Task<bool> UpdateAsync(Account account, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task SaveChangesAsync(CancellationToken cancellationToken);
}