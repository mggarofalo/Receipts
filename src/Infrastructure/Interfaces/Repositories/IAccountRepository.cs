using Application.Models;
using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface IAccountRepository
{
	Task<AccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<AccountEntity?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken);
	Task<List<AccountEntity>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<List<AccountEntity>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<int> GetDeletedCountAsync(CancellationToken cancellationToken);
	Task<List<AccountEntity>> CreateAsync(List<AccountEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<AccountEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
}
