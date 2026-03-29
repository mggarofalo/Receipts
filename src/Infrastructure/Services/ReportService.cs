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

	public async Task<SpendingByLocationResult> GetSpendingByLocationAsync(
		DateOnly? startDate,
		DateOnly? endDate,
		string sortBy,
		string sortDirection,
		int page,
		int pageSize,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		var receiptsQuery = context.Receipts.AsNoTracking()
			.Where(r => r.DeletedAt == null);

		if (startDate.HasValue)
		{
			receiptsQuery = receiptsQuery.Where(r => r.Date >= startDate.Value);
		}

		if (endDate.HasValue)
		{
			receiptsQuery = receiptsQuery.Where(r => r.Date <= endDate.Value);
		}

		var baseQuery = from r in receiptsQuery
						let transactionTotal = context.Transactions
							.Where(t => t.ReceiptId == r.Id && t.DeletedAt == null)
							.Sum(t => (decimal?)t.Amount) ?? 0m
						group new { TransactionTotal = transactionTotal } by (r.Location ?? "") into g
						select new
						{
							Location = g.Key == "" ? "(No Location)" : g.Key,
							Visits = g.Count(),
							Total = g.Sum(x => x.TransactionTotal),
						};

		var allItems = await baseQuery.ToListAsync(cancellationToken);
		int totalCount = allItems.Count;
		decimal grandTotal = allItems.Sum(x => x.Total);

		var sorted = (sortBy.ToLowerInvariant(), sortDirection.ToLowerInvariant()) switch
		{
			("location", "asc") => allItems.OrderBy(x => x.Location).AsEnumerable(),
			("location", "desc") => allItems.OrderByDescending(x => x.Location).AsEnumerable(),
			("visits", "asc") => allItems.OrderBy(x => x.Visits).AsEnumerable(),
			("visits", "desc") => allItems.OrderByDescending(x => x.Visits).AsEnumerable(),
			("averagepervisit", "asc") => allItems.OrderBy(x => x.Visits > 0 ? x.Total / x.Visits : 0).AsEnumerable(),
			("averagepervisit", "desc") => allItems.OrderByDescending(x => x.Visits > 0 ? x.Total / x.Visits : 0).AsEnumerable(),
			("total", "asc") => allItems.OrderBy(x => x.Total).AsEnumerable(),
			_ => allItems.OrderByDescending(x => x.Total).AsEnumerable(), // default: total desc
		};

		List<SpendingByLocationItem> pagedItems = sorted
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new SpendingByLocationItem(
				x.Location,
				x.Visits,
				x.Total,
				x.Visits > 0 ? Math.Round(x.Total / x.Visits, 2) : 0m))
			.ToList();

		return new SpendingByLocationResult(pagedItems, totalCount, grandTotal);
	}
}
