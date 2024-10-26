using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class AccountRepository(ApplicationDbContext context) : IAccountRepository
{
	public async Task<AccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Accounts.FindAsync([id], cancellationToken);
	}

	public async Task<AccountEntity?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken)
	{
		return await context.Transactions
			.Where(t => t.Id == transactionId)
			.Select(t => t.Account)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<List<AccountEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		return await context.Accounts
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AccountEntity>> CreateAsync(List<AccountEntity> entities, CancellationToken cancellationToken)
	{
		List<AccountEntity> createdEntities = [];

		foreach (AccountEntity entity in entities)
		{
			EntityEntry<AccountEntity> entityEntry = await context.Accounts.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities;
	}

	public async Task UpdateAsync(List<AccountEntity> entities, CancellationToken cancellationToken)
	{
		foreach (AccountEntity entity in entities)
		{
			AccountEntity existingEntity = await context.Accounts.SingleAsync(e => e.Id == entity.Id, cancellationToken);
			existingEntity.AccountCode = entity.AccountCode;
			existingEntity.Name = entity.Name;
			existingEntity.IsActive = entity.IsActive;
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<AccountEntity> entities = await context.Accounts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Accounts.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Accounts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.Accounts.CountAsync(cancellationToken);
	}
}