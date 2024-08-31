using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class AccountRepository(ApplicationDbContext context, IMapper mapper) : IAccountRepository
{
	public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		AccountEntity? entity = await context.Accounts
			.FindAsync([id], cancellationToken);

		return mapper.Map<Account>(entity);
	}

	public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<AccountEntity> entities = await context.Accounts
			.ToListAsync(cancellationToken);

		return entities.Select(mapper.Map<Account>).ToList();
	}

	public async Task<List<Account>> CreateAsync(List<Account> models, CancellationToken cancellationToken)
	{
		List<AccountEntity> createdEntities = [];

		foreach (AccountEntity entity in models.Select(mapper.Map<AccountEntity>).ToList())
		{
			EntityEntry<AccountEntity> entityEntry = await context.Accounts.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(mapper.Map<Account>).ToList();
	}

	public async Task<bool> UpdateAsync(List<Account> models, CancellationToken cancellationToken)
	{
		List<AccountEntity> newEntities = models.Select(mapper.Map<AccountEntity>).ToList();

		foreach (AccountEntity newEntity in newEntities)
		{
			AccountEntity existingEntity = await context.Accounts.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.AccountCode = newEntity.AccountCode;
			existingEntity.Name = newEntity.Name;
			existingEntity.IsActive = newEntity.IsActive;
		}

		return true;
	}

	public async Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<AccountEntity> entities = await context.Accounts.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		context.Accounts.RemoveRange(entities);

		return true;
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Accounts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.Accounts.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}
}