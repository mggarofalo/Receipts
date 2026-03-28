using System.Linq.Expressions;
using Application.Models;
using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReceiptRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IReceiptRepository
{
	private static readonly Dictionary<string, Expression<Func<ReceiptEntity, object>>> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
	{
		["location"] = e => e.Location,
		["date"] = e => e.Date,
		["taxAmount"] = e => e.TaxAmount,
	};

	public async Task<ReceiptEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts.FindAsync([id], cancellationToken);
	}

	public async Task<List<ReceiptEntity>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts
			.AsNoTracking()
			.ApplySort(sort, AllowedSortColumns, e => e.Date, defaultDescending: true)
			.Skip(offset)
			.Take(limit)
			.Select(r => new ReceiptEntity
			{
				Id = r.Id,
				Location = r.Location,
				Date = r.Date,
				TaxAmount = r.TaxAmount,
				TaxAmountCurrency = r.TaxAmountCurrency
			})
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ReceiptEntity>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts
			.OnlyDeleted()
			.AsNoTracking()
			.ApplySort(sort, AllowedSortColumns, e => e.Date, defaultDescending: true)
			.Skip(offset)
			.Take(limit)
			.Select(r => new ReceiptEntity
			{
				Id = r.Id,
				Location = r.Location,
				Date = r.Date,
				TaxAmount = r.TaxAmount,
				TaxAmountCurrency = r.TaxAmountCurrency,
				DeletedAt = r.DeletedAt
			})
			.ToListAsync(cancellationToken);
	}

	public async Task<int> GetDeletedCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts
			.OnlyDeleted()
			.CountAsync(cancellationToken);
	}

	public async Task<List<ReceiptEntity>> CreateAsync(List<ReceiptEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		context.Receipts.AddRange(entities);
		await context.SaveChangesAsync(cancellationToken);
		return entities;
	}

	public async Task UpdateAsync(List<ReceiptEntity> entities, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IEnumerable<Guid> ids = entities.Select(e => e.Id);
		List<ReceiptEntity> existingEntities = await context.Receipts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		foreach (ReceiptEntity entity in entities)
		{
			ReceiptEntity existingEntity = existingEntities.Single(e => e.Id == entity.Id);
			context.Entry(existingEntity).CurrentValues.SetValues(entity);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<ReceiptEntity> entities = await context.Receipts
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		// Load owned children into the change tracker so cascade soft-delete fires
		await context.ReceiptItems.IgnoreAutoIncludes().Where(i => ids.Contains(i.ReceiptId)).LoadAsync(cancellationToken);
		await context.Transactions.IgnoreAutoIncludes().Where(t => ids.Contains(t.ReceiptId)).LoadAsync(cancellationToken);
		await context.Adjustments.IgnoreAutoIncludes().Where(a => ids.Contains(a.ReceiptId)).LoadAsync(cancellationToken);

		context.Receipts.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		return await context.Receipts.CountAsync(cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		ReceiptEntity? entity = await context.Receipts
			.IncludeDeleted()
			.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt != null, cancellationToken);

		if (entity is null)
		{
			return false;
		}

		entity.DeletedAt = null;
		entity.DeletedByUserId = null;
		entity.DeletedByApiKeyId = null;
		entity.CascadeDeletedByParentId = null;

		await context.RestoreOwnedChildrenAsync<ReceiptEntity>(id, cancellationToken);

		await context.SaveChangesAsync(cancellationToken);
		return true;
	}
}
