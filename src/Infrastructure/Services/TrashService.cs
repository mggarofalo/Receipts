using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TrashService(ApplicationDbContext context) : ITrashService
{
	public async Task PurgeAllDeletedAsync(CancellationToken cancellationToken)
	{
		await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

		// Delete in FK dependency order (children first)
		await context.YnabSyncRecords
			.IgnoreQueryFilters()
			.Where(e => e.DeletedAt != null)
			.ExecuteDeleteAsync(cancellationToken);

		await context.Adjustments
			.IgnoreQueryFilters()
			.Where(e => e.DeletedAt != null)
			.ExecuteDeleteAsync(cancellationToken);

		await context.ReceiptItems
			.IgnoreQueryFilters()
			.Where(e => e.DeletedAt != null)
			.ExecuteDeleteAsync(cancellationToken);

		await context.Transactions
			.IgnoreQueryFilters()
			.Where(e => e.DeletedAt != null)
			.ExecuteDeleteAsync(cancellationToken);

		await context.Receipts
			.IgnoreQueryFilters()
			.Where(e => e.DeletedAt != null)
			.ExecuteDeleteAsync(cancellationToken);

		await context.ItemTemplates
			.IgnoreQueryFilters()
			.Where(e => e.DeletedAt != null)
			.ExecuteDeleteAsync(cancellationToken);

		// Delete both soft-deleted subcategories AND active subcategories
		// whose parent Category is about to be purged. The Subcategory → Category
		// FK is configured OnDelete(Cascade), so without this explicit step the
		// Category delete below would silently cascade-destroy any active
		// Subcategory rows pointing at a soft-deleted parent.
		await context.Subcategories
			.IgnoreQueryFilters()
			.Where(s => s.DeletedAt != null
				|| context.Categories
					.IgnoreQueryFilters()
					.Any(c => c.Id == s.CategoryId && c.DeletedAt != null))
			.ExecuteDeleteAsync(cancellationToken);

		await context.Categories
			.IgnoreQueryFilters()
			.Where(e => e.DeletedAt != null)
			.ExecuteDeleteAsync(cancellationToken);

		await transaction.CommitAsync(cancellationToken);
	}
}
