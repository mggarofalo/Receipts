using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class DashboardService(IDbContextFactory<ApplicationDbContext> contextFactory) : IDashboardService
{
	public async Task<DashboardSummaryResult> GetSummaryAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		IQueryable<ReceiptEntity> receiptsInRange = context.Receipts
			.AsNoTracking()
			.Where(r => r.Date >= startDate && r.Date <= endDate);

		int totalReceipts = await receiptsInRange.CountAsync(cancellationToken);

		if (totalReceipts == 0)
		{
			return new DashboardSummaryResult(0, 0, 0, new NameCountResult(null, 0), new NameCountResult(null, 0));
		}

		// Get receipt IDs in range for joining with transactions/items
		IQueryable<Guid> receiptIds = receiptsInRange.Select(r => r.Id);

		// Total spent = sum of all transaction amounts for receipts in range
		decimal totalSpent = await context.Transactions
			.AsNoTracking()
			.Where(t => receiptIds.Contains(t.ReceiptId))
			.SumAsync(t => t.Amount, cancellationToken);

		decimal averageTripAmount = totalReceipts > 0 ? Math.Round(totalSpent / totalReceipts, 2) : 0;

		// Most-used account = account with the most transactions in range
		// Single query: EF Core translates Account!.Name to a SQL JOIN, eliminating a second round-trip.
		var mostUsedAccountData = await context.Transactions
			.AsNoTracking()
			.Where(t => receiptIds.Contains(t.ReceiptId))
			.GroupBy(t => t.Account!.Name)
			.Select(g => new { AccountName = g.Key, Count = g.Count() })
			.OrderByDescending(g => g.Count)
			.FirstOrDefaultAsync(cancellationToken);

		NameCountResult mostUsedAccount = mostUsedAccountData != null
			? new NameCountResult(mostUsedAccountData.AccountName, mostUsedAccountData.Count)
			: new NameCountResult(null, 0);

		// Most-used category = category with the most receipt items in range
		var mostUsedCategoryData = await context.ReceiptItems
			.AsNoTracking()
			.Where(ri => receiptIds.Contains(ri.ReceiptId))
			.GroupBy(ri => ri.Category)
			.Select(g => new { Category = g.Key, Count = g.Count() })
			.OrderByDescending(g => g.Count)
			.FirstOrDefaultAsync(cancellationToken);

		NameCountResult mostUsedCategory = mostUsedCategoryData != null
			? new NameCountResult(mostUsedCategoryData.Category, mostUsedCategoryData.Count)
			: new NameCountResult(null, 0);

		return new DashboardSummaryResult(totalReceipts, totalSpent, averageTripAmount, mostUsedAccount, mostUsedCategory);
	}

	public async Task<SpendingOverTimeResult> GetSpendingOverTimeAsync(DateOnly startDate, DateOnly endDate, string granularity, CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		IQueryable<Guid> receiptIds = context.Receipts
			.AsNoTracking()
			.Where(r => r.Date >= startDate && r.Date <= endDate)
			.Select(r => r.Id);

		// Get all transactions for receipts in range, joined with receipt date
		var transactionsWithDate = context.Transactions
			.AsNoTracking()
			.Where(t => receiptIds.Contains(t.ReceiptId))
			.Join(
				context.Receipts.AsNoTracking(),
				t => t.ReceiptId,
				r => r.Id,
				(t, r) => new { t.Amount, r.Date });

		List<SpendingBucketResult> buckets;

		switch (granularity.ToLowerInvariant())
		{
			case "daily":
				buckets = await transactionsWithDate
					.GroupBy(x => x.Date)
					.Select(g => new SpendingBucketResult(g.Key.ToString("yyyy-MM-dd"), g.Sum(x => x.Amount)))
					.OrderBy(b => b.Period)
					.ToListAsync(cancellationToken);
				break;

			case "weekly":
				// Materialize first — DayOfWeek arithmetic cannot be translated to SQL by EF Core.
				// Then group by the Monday of each ISO week in memory.
				var weeklyRaw = await transactionsWithDate.ToListAsync(cancellationToken);
				buckets = weeklyRaw
					.GroupBy(x => x.Date.AddDays(-(((int)x.Date.DayOfWeek + 6) % 7)))
					.OrderBy(g => g.Key)
					.Select(g => new SpendingBucketResult(g.Key.ToString("yyyy-MM-dd"), g.Sum(x => x.Amount)))
					.ToList();
				break;

			case "monthly":
			default:
				var monthlyRaw = await transactionsWithDate
					.GroupBy(x => new { x.Date.Year, x.Date.Month })
					.Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(x => x.Amount) })
					.OrderBy(g => g.Year)
					.ThenBy(g => g.Month)
					.ToListAsync(cancellationToken);
				buckets = monthlyRaw
					.Select(m => new SpendingBucketResult($"{m.Year}-{m.Month:D2}", m.Amount))
					.ToList();
				break;
		}

		return new SpendingOverTimeResult(buckets);
	}

	public async Task<SpendingByCategoryResult> GetSpendingByCategoryAsync(DateOnly startDate, DateOnly endDate, int limit, CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		IQueryable<Guid> receiptIds = context.Receipts
			.AsNoTracking()
			.Where(r => r.Date >= startDate && r.Date <= endDate)
			.Select(r => r.Id);

		// Single query: materialize all categories (bounded by distinct category count, typically < 50),
		// then compute total in memory to avoid enumerating the IQueryable twice.
		var allCategories = await context.ReceiptItems
			.AsNoTracking()
			.Where(ri => receiptIds.Contains(ri.ReceiptId))
			.GroupBy(ri => ri.Category)
			.Select(g => new { Category = g.Key, Amount = g.Sum(ri => ri.TotalAmount) })
			.OrderByDescending(g => g.Amount)
			.ToListAsync(cancellationToken);

		decimal total = allCategories.Sum(c => c.Amount);

		var categorySpending = allCategories
			.Take(limit);

		List<SpendingCategoryItemResult> items = categorySpending
			.Select(c => new SpendingCategoryItemResult(
				c.Category,
				c.Amount,
				total > 0 ? Math.Round(c.Amount / total * 100, 2) : 0))
			.ToList();

		return new SpendingByCategoryResult(items);
	}

	public async Task<SpendingByAccountResult> GetSpendingByAccountAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		IQueryable<Guid> receiptIds = context.Receipts
			.AsNoTracking()
			.Where(r => r.Date >= startDate && r.Date <= endDate)
			.Select(r => r.Id);

		var accountSpending = await context.Transactions
			.AsNoTracking()
			.Where(t => receiptIds.Contains(t.ReceiptId))
			.GroupBy(t => t.AccountId)
			.Select(g => new { AccountId = g.Key, Amount = g.Sum(t => t.Amount) })
			.OrderByDescending(g => g.Amount)
			.ToListAsync(cancellationToken);

		decimal total = accountSpending.Sum(a => a.Amount);

		// Fetch account names in a single query
		List<Guid> accountIds = accountSpending.Select(a => a.AccountId).ToList();
		Dictionary<Guid, string> accountNames = await context.Accounts
			.AsNoTracking()
			.Where(a => accountIds.Contains(a.Id))
			.ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

		List<SpendingAccountItemResult> items = accountSpending
			.Select(a => new SpendingAccountItemResult(
				a.AccountId,
				accountNames.GetValueOrDefault(a.AccountId, "Unknown"),
				a.Amount,
				total > 0 ? Math.Round(a.Amount / total * 100, 2) : 0))
			.ToList();

		return new SpendingByAccountResult(items);
	}
}
