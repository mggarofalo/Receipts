using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class ReceiptItemRepository(ApplicationDbContext context, IMapper mapper) : IReceiptItemRepository
{
	public async Task<ReceiptItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptItemEntity? entity = await context.ReceiptItems
			.FindAsync([id], cancellationToken);

		return mapper.Map<ReceiptItem>(entity);
	}

	public async Task<List<ReceiptItem>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> receiptItemEntities = await context.ReceiptItems
			.Where(ri => ri.ReceiptId == receiptId)
			.AsNoTracking()
			.ToListAsync(cancellationToken);

		return receiptItemEntities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<List<ReceiptItem>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> receiptItemEntities = await context.ReceiptItems
			.AsNoTracking()
			.ToListAsync(cancellationToken);

		return receiptItemEntities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<List<ReceiptItem>> CreateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> createdEntities = [];

		foreach (ReceiptItemEntity entity in models.Select(m => mapper.MapToReceiptItemEntity(m, receiptId)))
		{
			EntityEntry<ReceiptItemEntity> entityEntry = await context.ReceiptItems.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task UpdateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> newEntities = models.Select(m => mapper.MapToReceiptItemEntity(m, receiptId)).ToList();

		foreach (ReceiptItemEntity newEntity in newEntities)
		{
			ReceiptItemEntity existingEntity = await context.ReceiptItems.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.ReceiptId = newEntity.ReceiptId;
			existingEntity.ReceiptItemCode = newEntity.ReceiptItemCode;
			existingEntity.Description = newEntity.Description;
			existingEntity.Quantity = newEntity.Quantity;
			existingEntity.UnitPrice = newEntity.UnitPrice;
			existingEntity.Category = newEntity.Category;
			existingEntity.Subcategory = newEntity.Subcategory;
			existingEntity.TotalAmount = newEntity.TotalAmount;
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