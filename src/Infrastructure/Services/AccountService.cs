using Application.Interfaces.Services;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;

namespace Infrastructure.Services;

public class AccountService(IAccountRepository repository, IMapper mapper) : IAccountService
{
	public async Task<List<Account>> CreateAsync(List<Account> models, CancellationToken cancellationToken)
	{
		List<AccountEntity> accountEntities = models.Select(mapper.Map<AccountEntity>).ToList();
		List<AccountEntity> createdAccountEntities = await repository.CreateAsync(accountEntities, cancellationToken);
		return createdAccountEntities.Select(mapper.Map<Account>).ToList();
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<AccountEntity> accountEntities = await repository.GetAllAsync(cancellationToken);
		return accountEntities.Select(mapper.Map<Account>).ToList();
	}

	public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		AccountEntity? accountEntity = await repository.GetByIdAsync(id, cancellationToken);
		return accountEntity == null ? null : mapper.Map<Account>(accountEntity);
	}

	public async Task<Account?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken)
	{
		AccountEntity? accountEntity = await repository.GetByTransactionIdAsync(transactionId, cancellationToken);
		return accountEntity == null ? null : mapper.Map<Account>(accountEntity);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<Account> models, CancellationToken cancellationToken)
	{
		List<AccountEntity> accountEntities = models.Select(mapper.Map<AccountEntity>).ToList();
		await repository.UpdateAsync(accountEntities, cancellationToken);
	}
}
