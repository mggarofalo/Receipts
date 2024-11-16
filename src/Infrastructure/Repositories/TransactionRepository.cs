using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class TransactionRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : ITransactionRepository
{
	public async Task<TransactionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Transactions.FindAsync([id], cancellationToken);
	}

	public async Task<List<TransactionEntity>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Transactions
			.Where(t => t.ReceiptId == receiptId)
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<TransactionEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Transactions
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<TransactionEntity>> CreateAsync(List<TransactionEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<TransactionEntity> createdEntities = [];

		foreach (TransactionEntity entity in entities)
		{
			EntityEntry<TransactionEntity> entityEntry = await context.Transactions.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities;
	}

	public async Task UpdateAsync(List<TransactionEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<TransactionEntity> existingEntities = await context.Transactions
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (TransactionEntity entity in entities)
		{
			TransactionEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<TransactionEntity> entities = await context.Transactions
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Transactions.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Transactions.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Transactions.CountAsync(cancellationToken);
	}
}