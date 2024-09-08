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

	public async Task<List<ReceiptItem>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> entities = await context.ReceiptItems
			.ToListAsync(cancellationToken);

		return entities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<List<ReceiptItem>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		bool receiptExists = await ExistsAsync(receiptId, cancellationToken);

		if (!receiptExists)
		{
			return null;
		}

		List<ReceiptItemEntity> entities = await context.ReceiptItems
			.Where(x => x.ReceiptId == receiptId)
			.ToListAsync(cancellationToken);

		return entities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<List<ReceiptItem>> CreateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken)
	{
		bool receiptExists = await ExistsAsync(receiptId, cancellationToken);

		if (!receiptExists)
		{
			throw new ArgumentException($"Receipt does not exist (ID: {receiptId})");
		}

		List<ReceiptItemEntity> createdEntities = [];

		foreach (ReceiptItemEntity entity in models.Select(domain => mapper.MapToReceiptItemEntity(domain, receiptId)).ToList())
		{
			EntityEntry<ReceiptItemEntity> entityEntry = await context.ReceiptItems.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task UpdateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken)
	{
		bool receiptExists = await ExistsAsync(receiptId, cancellationToken);

		if (!receiptExists)
		{
			throw new ArgumentException($"Receipt does not exist (ID: {receiptId})");
		}

		List<ReceiptItemEntity> newEntities = models.Select(domain => mapper.MapToReceiptItemEntity(domain, receiptId)).ToList();

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
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> entities = await context.ReceiptItems.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		context.ReceiptItems.RemoveRange(entities);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.ReceiptItems.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.ReceiptItems.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}
}