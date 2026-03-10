using System.Linq.Expressions;
using Application.Models;
using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoryRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : ICategoryRepository
{
	private static readonly Dictionary<string, Expression<Func<CategoryEntity, object>>> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
	{
		["name"] = e => e.Name,
	};

	public async Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Categories.FindAsync([id], cancellationToken);
	}

	public async Task<List<CategoryEntity>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Categories
			.AsNoTracking()
			.ApplySort(sort, AllowedSortColumns, e => e.Name)
			.Skip(offset)
			.Take(limit)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<CategoryEntity>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Categories
			.OnlyDeleted()
			.AsNoTracking()
			.ApplySort(sort, AllowedSortColumns, e => e.Name)
			.Skip(offset)
			.Take(limit)
			.ToListAsync(cancellationToken);
	}

	public async Task<int> GetDeletedCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Categories
			.OnlyDeleted()
			.CountAsync(cancellationToken);
	}

	public async Task<List<CategoryEntity>> CreateAsync(List<CategoryEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		context.Categories.AddRange(entities);
		await context.SaveChangesAsync(cancellationToken);
		return entities;
	}

	public async Task UpdateAsync(List<CategoryEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<CategoryEntity> existingEntities = await context.Categories
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (CategoryEntity entity in entities)
		{
			CategoryEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<CategoryEntity> entities = await context.Categories
			.Include(c => c.Subcategories)
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Categories.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Categories.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Categories.CountAsync(cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		CategoryEntity? entity = await context.Categories
			.IncludeDeleted()
			.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt != null, cancellationToken);

		if (entity is null)
		{
			return false;
		}

		entity.DeletedAt = null;
		entity.DeletedByUserId = null;
		entity.DeletedByApiKeyId = null;

		// Cascade restore subcategories
		List<SubcategoryEntity> deletedSubcategories = await context.Subcategories
			.IncludeDeleted()
			.Where(e => e.CategoryId == id && e.DeletedAt != null)
			.ToListAsync(cancellationToken);

		foreach (SubcategoryEntity subcategory in deletedSubcategories)
		{
			subcategory.DeletedAt = null;
			subcategory.DeletedByUserId = null;
			subcategory.DeletedByApiKeyId = null;
		}

		await context.SaveChangesAsync(cancellationToken);
		return true;
	}
}
