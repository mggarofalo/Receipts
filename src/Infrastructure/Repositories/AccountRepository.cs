using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class AccountRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IAccountRepository
{
	public async Task<AccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Accounts.FindAsync([id], cancellationToken);
	}

	public async Task<AccountEntity?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Transactions
			.Where(t => t.Id == transactionId)
			.Select(t => t.Account)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<List<AccountEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Accounts
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AccountEntity>> CreateAsync(List<AccountEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
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
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<AccountEntity> existingEntities = await context.Accounts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (AccountEntity entity in entities)
		{
			AccountEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<AccountEntity> entities = await context.Accounts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Accounts.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Accounts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Accounts.CountAsync(cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		AccountEntity? entity = await context.Accounts
			.IncludeDeleted()
			.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt != null, cancellationToken);

		if (entity is null)
		{
			return false;
		}

		entity.DeletedAt = null;
		entity.DeletedByUserId = null;
		entity.DeletedByApiKeyId = null;
		await context.SaveChangesAsync(cancellationToken);
		return true;
	}
}