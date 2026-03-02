using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class AdjustmentRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IAdjustmentRepository
{
	public async Task<AdjustmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Adjustments.FindAsync([id], cancellationToken);
	}

	public async Task<List<AdjustmentEntity>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Adjustments
			.Where(a => a.ReceiptId == receiptId)
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AdjustmentEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Adjustments
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AdjustmentEntity>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Adjustments
			.OnlyDeleted()
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AdjustmentEntity>> CreateAsync(List<AdjustmentEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<AdjustmentEntity> createdEntities = [];

		foreach (AdjustmentEntity entity in entities)
		{
			EntityEntry<AdjustmentEntity> entityEntry = await context.Adjustments.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities;
	}

	public async Task UpdateAsync(List<AdjustmentEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<AdjustmentEntity> existingEntities = await context.Adjustments
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (AdjustmentEntity entity in entities)
		{
			AdjustmentEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<AdjustmentEntity> entities = await context.Adjustments
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Adjustments.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Adjustments.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Adjustments.CountAsync(cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		AdjustmentEntity? entity = await context.Adjustments
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
