using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class ReceiptRepository(ApplicationDbContext context, IMapper mapper) : IReceiptRepository
{
	public async Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptEntity? entity = await context.Receipts
			.FindAsync([id], cancellationToken);

		return mapper.Map<Receipt>(entity);
	}

	public async Task<List<Receipt>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await context.Receipts
			.ToListAsync(cancellationToken);

		return entities.Select(mapper.Map<Receipt>).ToList();
	}

	public async Task<List<Receipt>> CreateAsync(List<Receipt> models, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> createdEntities = [];

		foreach (ReceiptEntity entity in models.Select(mapper.Map<ReceiptEntity>))
		{
			EntityEntry<ReceiptEntity> entityEntry = await context.Receipts.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(mapper.Map<Receipt>).ToList();
	}

	public async Task UpdateAsync(List<Receipt> models, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> newEntities = models.Select(mapper.Map<ReceiptEntity>).ToList();

		foreach (ReceiptEntity newEntity in newEntities)
		{
			ReceiptEntity existingEntity = await context.Receipts.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.Description = newEntity.Description;
			existingEntity.Location = newEntity.Location;
			existingEntity.Date = newEntity.Date;
			existingEntity.TaxAmount = newEntity.TaxAmount;
		}
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await context.Receipts.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		context.Receipts.RemoveRange(entities);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Receipts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.Receipts.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}
}