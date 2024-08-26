using Application.Interfaces;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class ReceiptItemRepository(ApplicationDbContext context, IMapper mapper) : IReceiptItemRepository
{
	private readonly ApplicationDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<ReceiptItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptItemEntity? entity = await _context.ReceiptItems
			.FindAsync([id], cancellationToken);

		return _mapper.Map<ReceiptItem>(entity);
	}

	public async Task<List<ReceiptItem>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> entities = await _context.ReceiptItems
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<List<ReceiptItem>> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> entities = await _context.ReceiptItems
			.Where(x => x.ReceiptId == receiptId)
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<List<ReceiptItem>> CreateAsync(List<ReceiptItem> models, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> createdEntities = [];

		foreach (ReceiptItemEntity entity in models.Select(_mapper.Map<ReceiptItemEntity>).ToList())
		{
			EntityEntry<ReceiptItemEntity> entityEntry = await _context.ReceiptItems.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(_mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<bool> UpdateAsync(List<ReceiptItem> models, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> newEntities = models.Select(_mapper.Map<ReceiptItemEntity>).ToList();

		foreach (ReceiptItemEntity newEntity in newEntities)
		{
			ReceiptItemEntity existingEntity = await _context.ReceiptItems.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.ReceiptId = newEntity.ReceiptId;
			existingEntity.ReceiptItemCode = newEntity.ReceiptItemCode;
			existingEntity.Description = newEntity.Description;
			existingEntity.Quantity = newEntity.Quantity;
			existingEntity.UnitPrice = newEntity.UnitPrice;
			existingEntity.Category = newEntity.Category;
			existingEntity.Subcategory = newEntity.Subcategory;
			existingEntity.TotalAmount = newEntity.TotalAmount;
		}

		return true;
	}

	public async Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> entities = await _context.ReceiptItems.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		_context.ReceiptItems.RemoveRange(entities);

		return true;
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await _context.ReceiptItems.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await _context.ReceiptItems.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await _context.SaveChangesAsync(cancellationToken);
	}
}