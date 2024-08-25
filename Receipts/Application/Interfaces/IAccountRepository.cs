using Domain.Core;

namespace Application.Interfaces;

public interface IAccountRepository
{
	Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<Account>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<Account>> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken);
	Task<List<Account>> GetByNameAsync(string name, CancellationToken cancellationToken);
	Task<List<Account>> CreateAsync(List<Account> accounts, CancellationToken cancellationToken);
	Task<bool> UpdateAsync(List<Account> accounts, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task SaveChangesAsync(CancellationToken cancellationToken);
}