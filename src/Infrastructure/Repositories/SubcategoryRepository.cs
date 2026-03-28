using System.Linq.Expressions;
using Application.Models;
using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SubcategoryRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : ISubcategoryRepository
{
	private static readonly Dictionary<string, Expression<Func<SubcategoryEntity, object>>> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
	{
		["name"] = e => e.Name,
	};

	public async Task<SubcategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Subcategories.FindAsync([id], cancellationToken);
	}

	public async Task<List<SubcategoryEntity>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Subcategories
			.AsNoTracking()
			.ApplySort(sort, AllowedSortColumns, e => e.Name)
			.Skip(offset)
			.Take(limit)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<SubcategoryEntity>> GetByCategoryIdAsync(Guid categoryId, int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Subcategories
			.Where(s => s.CategoryId == categoryId)
			.AsNoTracking()
			.ApplySort(sort, AllowedSortColumns, e => e.Name)
			.Skip(offset)
			.Take(limit)
			.ToListAsync(cancellationToken);
	}

	public async Task<int> GetByCategoryIdCountAsync(Guid categoryId, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Subcategories
			.Where(s => s.CategoryId == categoryId)
			.CountAsync(cancellationToken);
	}

	public async Task<List<SubcategoryEntity>> CreateAsync(List<SubcategoryEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		context.Subcategories.AddRange(entities);
		await context.SaveChangesAsync(cancellationToken);
		return entities;
	}

	public async Task UpdateAsync(List<SubcategoryEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<SubcategoryEntity> existingEntities = await context.Subcategories
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (SubcategoryEntity entity in entities)
		{
			SubcategoryEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Subcategories.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Subcategories.CountAsync(cancellationToken);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		SubcategoryEntity? entity = await context.Subcategories.FindAsync([id], cancellationToken);
		if (entity != null)
		{
			context.Subcategories.Remove(entity);
			await context.SaveChangesAsync(cancellationToken);
		}
	}

	public async Task<int> GetReceiptItemCountBySubcategoryNameAsync(string subcategoryName, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.ReceiptItems
			.IgnoreQueryFilters()
			.CountAsync(ri => ri.Subcategory == subcategoryName, cancellationToken);
	}
}
