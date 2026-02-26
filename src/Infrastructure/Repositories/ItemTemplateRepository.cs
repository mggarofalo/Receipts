using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class ItemTemplateRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IItemTemplateRepository
{
	public async Task<ItemTemplateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.ItemTemplates.FindAsync([id], cancellationToken);
	}

	public async Task<List<ItemTemplateEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.ItemTemplates
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ItemTemplateEntity>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.ItemTemplates
			.OnlyDeleted()
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ItemTemplateEntity>> CreateAsync(List<ItemTemplateEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<ItemTemplateEntity> createdEntities = [];

		foreach (ItemTemplateEntity entity in entities)
		{
			EntityEntry<ItemTemplateEntity> entityEntry = await context.ItemTemplates.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities;
	}

	public async Task UpdateAsync(List<ItemTemplateEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<ItemTemplateEntity> existingEntities = await context.ItemTemplates
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (ItemTemplateEntity entity in entities)
		{
			ItemTemplateEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<ItemTemplateEntity> entities = await context.ItemTemplates
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.ItemTemplates.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.ItemTemplates.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.ItemTemplates.CountAsync(cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		ItemTemplateEntity? entity = await context.ItemTemplates
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
