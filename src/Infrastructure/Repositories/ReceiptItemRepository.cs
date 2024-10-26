using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

// TODO: Handle cases where a caller sends entities that haven't been saved yet to methods that expect saved entities

public class ReceiptItemRepository(ApplicationDbContext context) : IReceiptItemRepository
{
	public async Task<ReceiptItemEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.ReceiptItems.FindAsync([id], cancellationToken);
	}

	public async Task<List<ReceiptItemEntity>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		return await context.ReceiptItems
			.Where(ri => ri.ReceiptId == receiptId)
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ReceiptItemEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		return await context.ReceiptItems
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ReceiptItemEntity>> CreateAsync(List<ReceiptItemEntity> entities, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> createdEntities = [];

		foreach (ReceiptItemEntity entity in entities)
		{
			EntityEntry<ReceiptItemEntity> entityEntry = await context.ReceiptItems.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities;
	}

	public async Task UpdateAsync(List<ReceiptItemEntity> entities, CancellationToken cancellationToken)
	{
		foreach (ReceiptItemEntity entity in entities)
		{
			ReceiptItemEntity existingEntity = await context.ReceiptItems.SingleAsync(e => e.Id == entity.Id, cancellationToken);
			existingEntity.ReceiptId = entity.ReceiptId;
			existingEntity.ReceiptItemCode = entity.ReceiptItemCode;
			existingEntity.Description = entity.Description;
			existingEntity.Quantity = entity.Quantity;
			existingEntity.UnitPrice = entity.UnitPrice;
			existingEntity.Category = entity.Category;
			existingEntity.Subcategory = entity.Subcategory;
			existingEntity.TotalAmount = entity.TotalAmount;
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> entities = await context.ReceiptItems
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.ReceiptItems.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.ReceiptItems.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.ReceiptItems.CountAsync(cancellationToken);
	}
}