using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface IAccountRepository
{
	Task<AccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<AccountEntity?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken);
	Task<List<AccountEntity>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<AccountEntity>> CreateAsync(List<AccountEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<AccountEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
}
