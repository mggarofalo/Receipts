using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class ReceiptRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IReceiptRepository
{
	public async Task<ReceiptEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts.FindAsync([id], cancellationToken);
	}

	public async Task<List<ReceiptEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ReceiptEntity>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts
			.OnlyDeleted()
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ReceiptEntity>> CreateAsync(List<ReceiptEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<ReceiptEntity> createdEntities = [];

		foreach (ReceiptEntity entity in entities)
		{
			EntityEntry<ReceiptEntity> entityEntry = await context.Receipts.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities;
	}

	public async Task UpdateAsync(List<ReceiptEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<ReceiptEntity> existingEntities = await context.Receipts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (ReceiptEntity entity in entities)
		{
			ReceiptEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<ReceiptEntity> entities = await context.Receipts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Receipts.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts.CountAsync(cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		ReceiptEntity? entity = await context.Receipts
			.IncludeDeleted()
			.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt != null, cancellationToken);

		if (entity is null)
		{
			return false;
		}

		entity.DeletedAt = null;
		entity.DeletedByUserId = null;
		entity.DeletedByApiKeyId = null;

		// Cascade restore receipt items
		List<ReceiptItemEntity> deletedItems = await context.ReceiptItems
			.IncludeDeleted()
			.Where(e => e.ReceiptId == id && e.DeletedAt != null)
			.ToListAsync(cancellationToken);

		foreach (ReceiptItemEntity item in deletedItems)
		{
			item.DeletedAt = null;
			item.DeletedByUserId = null;
			item.DeletedByApiKeyId = null;
		}

		// Cascade restore transactions
		List<TransactionEntity> deletedTransactions = await context.Transactions
			.IncludeDeleted()
			.Where(e => e.ReceiptId == id && e.DeletedAt != null)
			.ToListAsync(cancellationToken);

		foreach (TransactionEntity transaction in deletedTransactions)
		{
			transaction.DeletedAt = null;
			transaction.DeletedByUserId = null;
			transaction.DeletedByApiKeyId = null;
		}

		await context.SaveChangesAsync(cancellationToken);
		return true;
	}
}