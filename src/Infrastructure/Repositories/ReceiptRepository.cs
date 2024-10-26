using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

// TODO: Handle cases where a caller sends entities that haven't been saved yet to methods that expect saved entities

public class ReceiptRepository(ApplicationDbContext context) : IReceiptRepository
{
	public async Task<ReceiptEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Receipts.FindAsync([id], cancellationToken);
	}

	public async Task<List<ReceiptEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		return await context.Receipts
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ReceiptEntity>> CreateAsync(List<ReceiptEntity> entities, CancellationToken cancellationToken)
	{
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
		foreach (ReceiptEntity entity in entities)
		{
			ReceiptEntity existingEntity = await context.Receipts.SingleAsync(e => e.Id == entity.Id, cancellationToken);
			existingEntity.Description = entity.Description;
			existingEntity.Location = entity.Location;
			existingEntity.Date = entity.Date;
			existingEntity.TaxAmount = entity.TaxAmount;
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await context.Receipts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Receipts.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Receipts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.Receipts.CountAsync(cancellationToken);
	}
}