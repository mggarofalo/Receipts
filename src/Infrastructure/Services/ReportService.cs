using Application.Interfaces.Services;
using Application.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ReportService(IDbContextFactory<ApplicationDbContext> contextFactory) : IReportService
{
	public async Task<OutOfBalanceResult> GetOutOfBalanceAsync(
		string sortBy,
		string sortDirection,
		int page,
		int pageSize,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		// Build the base query: JOIN receipts with aggregated items, transactions, and adjustments.
		// Use LEFT JOINs so receipts with no items/transactions/adjustments still appear if out of balance.
		var baseQuery = from r in context.Receipts.AsNoTracking()
						where r.DeletedAt == null
						let itemSubtotal = context.ReceiptItems
							.Where(ri => ri.ReceiptId == r.Id && ri.DeletedAt == null)
							.Sum(ri => (decimal?)ri.TotalAmount) ?? 0m
						let adjustmentTotal = context.Adjustments
							.Where(a => a.ReceiptId == r.Id && a.DeletedAt == null)
							.Sum(a => (decimal?)a.Amount) ?? 0m
						let transactionTotal = context.Transactions
							.Where(t => t.ReceiptId == r.Id && t.DeletedAt == null)
							.Sum(t => (decimal?)t.Amount) ?? 0m
						let expectedTotal = itemSubtotal + r.TaxAmount + adjustmentTotal
						let difference = expectedTotal - transactionTotal
						where difference != 0m
						select new
						{
							r.Id,
							r.Location,
							r.Date,
							ItemSubtotal = itemSubtotal,
							TaxAmount = r.TaxAmount,
							AdjustmentTotal = adjustmentTotal,
							ExpectedTotal = expectedTotal,
							TransactionTotal = transactionTotal,
							Difference = difference
						};

		// Get total count and total discrepancy before pagination
		// Materialize the filtered set once so both aggregations happen in a single enumeration.
		var allItems = await baseQuery.ToListAsync(cancellationToken);
		int totalCount = allItems.Count;
		decimal totalDiscrepancy = allItems.Sum(x => Math.Abs(x.Difference));

		// Apply sorting in memory (the data set is bounded by "out of balance" receipts, typically small)
		var sorted = (sortBy.ToLowerInvariant(), sortDirection.ToLowerInvariant()) switch
		{
			("difference", "asc") => allItems.OrderBy(x => x.Difference).AsEnumerable(),
			("difference", "desc") => allItems.OrderByDescending(x => x.Difference).AsEnumerable(),
			("date", "desc") => allItems.OrderByDescending(x => x.Date).AsEnumerable(),
			_ => allItems.OrderBy(x => x.Date).AsEnumerable(), // default: date asc
		};

		List<OutOfBalanceItem> pagedItems = sorted
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new OutOfBalanceItem(
				x.Id,
				x.Location,
				x.Date,
				x.ItemSubtotal,
				x.TaxAmount,
				x.AdjustmentTotal,
				x.ExpectedTotal,
				x.TransactionTotal,
				x.Difference))
			.ToList();

		return new OutOfBalanceResult(pagedItems, totalCount, totalDiscrepancy);
	}
}
