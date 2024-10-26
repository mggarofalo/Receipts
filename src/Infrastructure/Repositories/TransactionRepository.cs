using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

// TODO: Handle cases where a caller sends entities that haven't been saved yet to methods that expect saved entities

public class TransactionRepository(ApplicationDbContext context) : ITransactionRepository
{
	public async Task<TransactionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Transactions.FindAsync([id], cancellationToken);
	}

	public async Task<List<TransactionEntity>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		return await context.Transactions
			.Where(t => t.ReceiptId == receiptId)
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<TransactionEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		return await context.Transactions
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<TransactionEntity>> CreateAsync(List<TransactionEntity> entities, CancellationToken cancellationToken)
	{
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
		foreach (TransactionEntity entity in entities)
		{
			TransactionEntity existingEntity = await context.Transactions.SingleAsync(e => e.Id == entity.Id, cancellationToken);
			existingEntity.ReceiptId = entity.ReceiptId;
			existingEntity.AccountId = entity.AccountId;
			existingEntity.Amount = entity.Amount;
			existingEntity.Date = entity.Date;
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await context.Transactions
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Transactions.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Transactions.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.Transactions.CountAsync(cancellationToken);
	}
}