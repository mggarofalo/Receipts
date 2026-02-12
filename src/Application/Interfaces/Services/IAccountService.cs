using Domain.Core;

namespace Application.Interfaces.Services;

public interface IAccountService : IService<Account>
{
	Task<Account?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken);
	Task<List<Account>> CreateAsync(List<Account> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<Account> models, CancellationToken cancellationToken);
}