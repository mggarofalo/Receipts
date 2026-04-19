using System.Text.RegularExpressions;
using Application.Interfaces.Services;
using Application.Models.Reports;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Services;

public partial class ReportService(IDbContextFactory<ApplicationDbContext> contextFactory) : IReportService
{
	[GeneratedRegex(@"\s+")]
	private static partial Regex WhitespaceRegex();
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

	public async Task<ItemSimilarityResult> GetItemSimilarityAsync(
		double threshold,
		string sortBy,
		string sortDirection,
		int page,
		int pageSize,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		// Step 1: Find similar pairs using DISTINCT descriptions to avoid O(n^2) on all items.
		// With ~2000 items but only ~100-300 unique descriptions, this reduces comparisons dramatically.
		const string sql = """
			WITH distinct_descs AS (
				SELECT "Description", COUNT(*) AS "OccurrenceCount"
				FROM "ReceiptItems"
				WHERE "DeletedAt" IS NULL
				GROUP BY "Description"
			)
			SELECT
				a."Description" AS "DescA",
				a."OccurrenceCount" AS "CountA",
				b."Description" AS "DescB",
				b."OccurrenceCount" AS "CountB",
				similarity(a."Description", b."Description") AS "Score"
			FROM distinct_descs a
			JOIN distinct_descs b
				ON a."Description" < b."Description"
				AND similarity(a."Description", b."Description") >= @threshold
			""";

		List<DescriptionSimilarityEdge> edges = [];

		// The connection returned by GetDbConnection() is owned by the DbContext — do not dispose it.
		// EF still needs this connection for the ReceiptItems query later in this method.
		NpgsqlConnection connection = (NpgsqlConnection)context.Database.GetDbConnection();
		await context.Database.OpenConnectionAsync(cancellationToken);

		await using NpgsqlCommand command = new(sql, connection);
		command.Parameters.AddWithValue("@threshold", threshold);

		await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
		{
			while (await reader.ReadAsync(cancellationToken))
			{
				edges.Add(new DescriptionSimilarityEdge(
					reader.GetString(0),
					(int)reader.GetInt64(1),
					reader.GetString(2),
					(int)reader.GetInt64(3),
					reader.GetDouble(4)));
			}
		}

		if (edges.Count == 0)
		{
			return new ItemSimilarityResult([], 0);
		}

		// Step 2: Build connected components via union-find on description strings.
		Dictionary<string, int> descriptionCounts = [];
		foreach (DescriptionSimilarityEdge edge in edges)
		{
			descriptionCounts.TryAdd(edge.DescA, edge.CountA);
			descriptionCounts.TryAdd(edge.DescB, edge.CountB);
		}

		Dictionary<string, string> parent = [];
		Dictionary<string, int> rank = [];

		foreach (string desc in descriptionCounts.Keys)
		{
			parent[desc] = desc;
			rank[desc] = 0;
		}

		string Find(string x)
		{
			while (parent[x] != x)
			{
				parent[x] = parent[parent[x]]; // path compression
				x = parent[x];
			}
			return x;
		}

		void Union(string x, string y)
		{
			string rx = Find(x);
			string ry = Find(y);
			if (rx == ry)
			{
				return;
			}

			if (rank[rx] < rank[ry])
			{
				parent[rx] = ry;
			}
			else if (rank[rx] > rank[ry])
			{
				parent[ry] = rx;
			}
			else
			{
				parent[ry] = rx;
				rank[rx]++;
			}
		}

		Dictionary<string, double> clusterMaxSimilarity = [];

		foreach (DescriptionSimilarityEdge edge in edges)
		{
			Union(edge.DescA, edge.DescB);
		}

		// Step 3: Build clusters from the union-find structure.
		Dictionary<string, List<string>> clusters = [];
		foreach (string desc in descriptionCounts.Keys)
		{
			string root = Find(desc);
			if (!clusters.TryGetValue(root, out List<string>? members))
			{
				members = [];
				clusters[root] = members;
			}
			members.Add(desc);
		}

		// Compute max similarity per cluster
		foreach (DescriptionSimilarityEdge edge in edges)
		{
			string root = Find(edge.DescA);
			if (!clusterMaxSimilarity.TryGetValue(root, out double current) || edge.Score > current)
			{
				clusterMaxSimilarity[root] = edge.Score;
			}
		}

		// Step 4: Collect all descriptions that appear in multi-description clusters
		// so we can look up their item IDs in a single query.
		HashSet<string> clusterDescriptions = [];
		foreach ((string root, List<string> members) in clusters)
		{
			if (members.Count < 2)
			{
				continue;
			}

			foreach (string desc in members)
			{
				clusterDescriptions.Add(desc);
			}
		}

		// Look up item IDs for all clustered descriptions in one query
		Dictionary<string, List<Guid>> descriptionItemIds = [];
		if (clusterDescriptions.Count > 0)
		{
			var itemLookup = await context.ReceiptItems
				.AsNoTracking()
				.Where(ri => ri.DeletedAt == null && clusterDescriptions.Contains(ri.Description))
				.Select(ri => new { ri.Id, ri.Description })
				.ToListAsync(cancellationToken);

			foreach (var item in itemLookup)
			{
				if (!descriptionItemIds.TryGetValue(item.Description, out List<Guid>? ids))
				{
					ids = [];
					descriptionItemIds[item.Description] = ids;
				}
				ids.Add(item.Id);
			}
		}

		// Step 5: Build result groups.
		List<ItemSimilarityGroup> groups = [];
		foreach ((string root, List<string> members) in clusters)
		{
			if (members.Count < 2)
			{
				continue;
			}

			// Collect all item IDs across all descriptions in this cluster
			List<Guid> allItemIds = [];
			Dictionary<string, int> descFrequency = [];
			foreach (string desc in members)
			{
				int count = descriptionCounts.GetValueOrDefault(desc, 0);
				descFrequency[desc] = count;
				if (descriptionItemIds.TryGetValue(desc, out List<Guid>? ids))
				{
					allItemIds.AddRange(ids);
				}
			}

			// Canonical = most frequent, ties broken by longest string
			string canonical = descFrequency
				.OrderByDescending(kv => kv.Value)
				.ThenByDescending(kv => kv.Key.Length)
				.First()
				.Key;

			List<string> variants = descFrequency.Keys.OrderBy(d => d).ToList();
			double maxSim = clusterMaxSimilarity.GetValueOrDefault(root, 0.0);

			groups.Add(new ItemSimilarityGroup(
				canonical,
				variants,
				allItemIds,
				allItemIds.Count,
				maxSim));
		}

		// Step 6: Sort
		IEnumerable<ItemSimilarityGroup> sorted = (sortBy.ToLowerInvariant(), sortDirection.ToLowerInvariant()) switch
		{
			("canonicalname", "asc") => groups.OrderBy(g => g.CanonicalName),
			("canonicalname", "desc") => groups.OrderByDescending(g => g.CanonicalName),
			("occurrences", "asc") => groups.OrderBy(g => g.Occurrences),
			("maxsimilarity", "asc") => groups.OrderBy(g => g.MaxSimilarity),
			("maxsimilarity", "desc") => groups.OrderByDescending(g => g.MaxSimilarity),
			_ => groups.OrderByDescending(g => g.Occurrences), // default: occurrences desc
		};

		int totalCount = groups.Count;

		// Step 7: Paginate
		List<ItemSimilarityGroup> pagedGroups = sorted
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return new ItemSimilarityResult(pagedGroups, totalCount);
	}

	public async Task<int> RenameItemsAsync(
		List<Guid> itemIds,
		string newDescription,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		int updated = await context.ReceiptItems
			.Where(ri => itemIds.Contains(ri.Id) && ri.DeletedAt == null)
			.ExecuteUpdateAsync(
				s => s.SetProperty(ri => ri.Description, newDescription),
				cancellationToken);

		return updated;
	}

	public async Task<ItemDescriptionResult> GetItemDescriptionsAsync(
		string search,
		bool categoryOnly,
		int limit,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		string searchLower = search.ToLower();

		if (categoryOnly)
		{
			var categoryResults = await context.ReceiptItems
				.AsNoTracking()
				.Where(ri => ri.DeletedAt == null && ri.Category.ToLower().Contains(searchLower))
				.GroupBy(ri => ri.Category)
				.Select(g => new { Category = g.Key, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.Take(limit)
				.ToListAsync(cancellationToken);

			List<ItemDescriptionItem> categories = categoryResults
				.Select(x => new ItemDescriptionItem(x.Category, x.Category, x.Count))
				.ToList();

			return new ItemDescriptionResult(categories);
		}

		var results = await context.ReceiptItems
			.AsNoTracking()
			.Where(ri => ri.DeletedAt == null && ri.Description.ToLower().Contains(searchLower))
			.GroupBy(ri => new { ri.Description, ri.Category })
			.Select(g => new { g.Key.Description, g.Key.Category, Count = g.Count() })
			.OrderByDescending(x => x.Count)
			.Take(limit)
			.ToListAsync(cancellationToken);

		List<ItemDescriptionItem> items = results
			.Select(x => new ItemDescriptionItem(x.Description, x.Category, x.Count))
			.ToList();

		return new ItemDescriptionResult(items);
	}

	public async Task<ItemCostOverTimeResult> GetItemCostOverTimeAsync(
		string? description,
		string? category,
		DateOnly? startDate,
		DateOnly? endDate,
		string granularity,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		var query = context.ReceiptItems
			.AsNoTracking()
			.Where(ri => ri.DeletedAt == null)
			.Join(
				context.Receipts.AsNoTracking().Where(r => r.DeletedAt == null),
				ri => ri.ReceiptId,
				r => r.Id,
				(ri, r) => new { ri.Description, ri.Category, ri.TotalAmount, ri.Quantity, ri.UnitPrice, r.Date });

		if (!string.IsNullOrEmpty(description))
		{
			string descLower = description.ToLower();
			query = query.Where(x => x.Description.ToLower() == descLower);
		}
		else if (!string.IsNullOrEmpty(category))
		{
			string catLower = category.ToLower();
			query = query.Where(x => x.Category.ToLower() == catLower);
		}

		if (startDate.HasValue)
		{
			query = query.Where(x => x.Date >= startDate.Value);
		}

		if (endDate.HasValue)
		{
			query = query.Where(x => x.Date <= endDate.Value);
		}

		List<ItemCostBucket> buckets;

		switch (granularity.ToLowerInvariant())
		{
			case "monthly":
				var monthlyData = await query
					.GroupBy(x => new { x.Date.Year, x.Date.Month })
					.Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Average(x => x.UnitPrice) })
					.OrderBy(x => x.Year).ThenBy(x => x.Month)
					.ToListAsync(cancellationToken);

				buckets = monthlyData
					.Select(x => new ItemCostBucket($"{x.Year}-{x.Month:D2}", x.Amount))
					.ToList();
				break;

			case "yearly":
				var yearlyData = await query
					.GroupBy(x => x.Date.Year)
					.Select(g => new { Year = g.Key, Amount = g.Average(x => x.UnitPrice) })
					.OrderBy(x => x.Year)
					.ToListAsync(cancellationToken);

				buckets = yearlyData
					.Select(x => new ItemCostBucket(x.Year.ToString(), x.Amount))
					.ToList();
				break;

			default: // "exact"
				var exactData = await query
					.Select(x => new { x.Date, x.UnitPrice })
					.OrderBy(x => x.Date)
					.ToListAsync(cancellationToken);

				buckets = exactData
					.Select(x => new ItemCostBucket(x.Date.ToString("yyyy-MM-dd"), x.UnitPrice))
					.ToList();
				break;
		}

		return new ItemCostOverTimeResult(buckets);
	}

	public async Task<DuplicateDetectionResult> GetDuplicatesAsync(
		string matchOn,
		string locationTolerance,
		decimal totalTolerance,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		List<ReceiptSnapshot> receipts = await (from r in context.Receipts.AsNoTracking()
												where r.DeletedAt == null
												let transactionTotal = context.Transactions
													.Where(t => t.ReceiptId == r.Id && t.DeletedAt == null)
													.Sum(t => (decimal?)t.Amount) ?? 0m
												select new ReceiptSnapshot(
													r.Id,
													r.Location,
													r.Date,
													transactionTotal
												)).ToListAsync(cancellationToken);

		bool normalized = locationTolerance.Equals("normalized", StringComparison.OrdinalIgnoreCase);

		string NormalizeLocation(string location)
		{
			string trimmed = location.Trim().ToLowerInvariant();
			return WhitespaceRegex().Replace(trimmed, " ");
		}

		bool TotalsMatch(decimal a, decimal b) => Math.Abs(a - b) <= totalTolerance;

		List<DuplicateGroup> groups = matchOn.ToLowerInvariant() switch
		{
			"dateandlocation" => receipts
				.GroupBy(r => (r.Date, Location: normalized ? NormalizeLocation(r.Location) : r.Location))
				.Where(g => g.Count() > 1)
				.Select(g => new DuplicateGroup(
					$"{g.Key.Date:yyyy-MM-dd} @ {g.First().Location}",
					g.Select(ToSummary).ToList()))
				.ToList(),

			"dateandtotal" => ClusterByTotal(
				receipts.GroupBy(r => r.Date),
				TotalsMatch,
				dateGroup => dateGroup.Key,
				(date, seed) => $"{date:yyyy-MM-dd} — ${seed.TransactionTotal:F2}"),

			_ => ClusterByTotal(
				receipts.GroupBy(r => (r.Date, Location: normalized ? NormalizeLocation(r.Location) : r.Location)),
				TotalsMatch,
				locDateGroup => locDateGroup.Key,
				(key, seed) => $"{key.Date:yyyy-MM-dd} @ {seed.Location} — ${seed.TransactionTotal:F2}")
		};

		int totalDuplicateReceipts = groups.Sum(g => g.Receipts.Count);
		return new DuplicateDetectionResult(groups, groups.Count, totalDuplicateReceipts);
	}

	private static List<DuplicateGroup> ClusterByTotal<TKey>(
		IEnumerable<IGrouping<TKey, ReceiptSnapshot>> groupedReceipts,
		Func<decimal, decimal, bool> totalsMatch,
		Func<IGrouping<TKey, ReceiptSnapshot>, TKey> keySelector,
		Func<TKey, ReceiptSnapshot, string> formatMatchKey)
	{
		List<DuplicateGroup> result = [];

		foreach (IGrouping<TKey, ReceiptSnapshot> group in groupedReceipts)
		{
			List<ReceiptSnapshot> remaining = [.. group];
			TKey key = keySelector(group);

			while (remaining.Count > 0)
			{
				ReceiptSnapshot seed = remaining[0];
				List<ReceiptSnapshot> cluster = [seed];
				remaining.RemoveAt(0);

				for (int i = remaining.Count - 1; i >= 0; i--)
				{
					if (totalsMatch(seed.TransactionTotal, remaining[i].TransactionTotal))
					{
						cluster.Add(remaining[i]);
						remaining.RemoveAt(i);
					}
				}

				if (cluster.Count > 1)
				{
					result.Add(new DuplicateGroup(
						formatMatchKey(key, seed),
						cluster.Select(ToSummary).ToList()));
				}
			}
		}

		return result;
	}

	private static DuplicateReceiptSummary ToSummary(ReceiptSnapshot r) =>
		new(r.Id, r.Location, r.Date, r.TransactionTotal);

	private sealed record ReceiptSnapshot(Guid Id, string Location, DateOnly Date, decimal TransactionTotal);

	public async Task<CategoryTrendsResult> GetCategoryTrendsAsync(
		DateOnly startDate,
		DateOnly endDate,
		string granularity,
		int topN,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		// Join receipt items with receipts for date filtering
		var itemsInRange = context.ReceiptItems
			.AsNoTracking()
			.Where(ri => ri.DeletedAt == null)
			.Join(
				context.Receipts.AsNoTracking().Where(r => r.DeletedAt == null && r.Date >= startDate && r.Date <= endDate),
				ri => ri.ReceiptId,
				r => r.Id,
				(ri, r) => new { ri.Category, ri.TotalAmount, r.Date });

		// Materialize for in-memory grouping (bounded by item count in date range)
		var materialized = await itemsInRange.ToListAsync(cancellationToken);

		if (materialized.Count == 0)
		{
			return new CategoryTrendsResult([], []);
		}

		// Find top-N categories by total spending
		var categoryTotals = materialized
			.GroupBy(x => x.Category)
			.Select(g => new { Category = g.Key, Total = g.Sum(x => x.TotalAmount) })
			.OrderByDescending(x => x.Total)
			.ToList();

		HashSet<string> topCategories = categoryTotals
			.Take(topN)
			.Select(x => x.Category)
			.ToHashSet();

		bool hasOther = categoryTotals.Count > topN;

		// Build ordered category list
		List<string> categories = categoryTotals
			.Take(topN)
			.Select(x => x.Category)
			.ToList();

		if (hasOther)
		{
			categories.Add("Other");
		}

		// Map items to resolved category (top-N or "Other")
		var resolvedItems = materialized.Select(x => new
		{
			Category = topCategories.Contains(x.Category) ? x.Category : "Other",
			x.TotalAmount,
			x.Date
		});

		// Generate all periods in range
		List<string> allPeriods = GeneratePeriods(startDate, endDate, granularity);

		// Group by period and category
		var grouped = resolvedItems
			.GroupBy(x => new { Period = FormatPeriod(x.Date, granularity), x.Category })
			.ToDictionary(g => (g.Key.Period, g.Key.Category), g => g.Sum(x => x.TotalAmount));

		// Build dense zero-filled buckets
		List<CategoryTrendsBucketResult> buckets = allPeriods.Select(period =>
		{
			List<decimal> amounts = categories.Select(cat =>
				grouped.TryGetValue((period, cat), out decimal amount) ? amount : 0m
			).ToList();
			return new CategoryTrendsBucketResult(period, amounts);
		}).ToList();

		return new CategoryTrendsResult(categories, buckets);
	}

	private static string FormatPeriod(DateOnly date, string granularity)
	{
		return granularity.ToLowerInvariant() switch
		{
			"daily" => date.ToString("yyyy-MM-dd"),
			"quarterly" => $"{date.Year} Q{(date.Month - 1) / 3 + 1}",
			"yearly" => date.Year.ToString(),
			_ => $"{date.Year}-{date.Month:D2}" // monthly
		};
	}

	private static List<string> GeneratePeriods(DateOnly start, DateOnly end, string granularity)
	{
		List<string> periods = [];
		DateOnly current = granularity.ToLowerInvariant() switch
		{
			"daily" => start,
			"quarterly" => new DateOnly(start.Year, ((start.Month - 1) / 3) * 3 + 1, 1),
			"yearly" => new DateOnly(start.Year, 1, 1),
			_ => new DateOnly(start.Year, start.Month, 1) // monthly
		};

		while (current <= end)
		{
			periods.Add(FormatPeriod(current, granularity));
			current = granularity.ToLowerInvariant() switch
			{
				"daily" => current.AddDays(1),
				"quarterly" => current.AddMonths(3),
				"yearly" => current.AddYears(1),
				_ => current.AddMonths(1) // monthly
			};
		}

		return periods;
	}

	public async Task<UncategorizedItemsResult> GetUncategorizedItemsAsync(
		string sortBy,
		string sortDirection,
		int page,
		int pageSize,
		CancellationToken cancellationToken)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		var baseQuery = from ri in context.ReceiptItems.AsNoTracking()
						where ri.DeletedAt == null && ri.Category == "Uncategorized"
						select new
						{
							ri.Id,
							ri.ReceiptId,
							ri.ReceiptItemCode,
							ri.Description,
							ri.Quantity,
							ri.UnitPrice,
							ri.TotalAmount,
							ri.Category,
							ri.Subcategory,
							ri.PricingMode
						};

		var allItems = await baseQuery.ToListAsync(cancellationToken);
		int totalCount = allItems.Count;

		var sorted = (sortBy.ToLowerInvariant(), sortDirection.ToLowerInvariant()) switch
		{
			("total", "asc") => allItems.OrderBy(x => x.TotalAmount).AsEnumerable(),
			("total", "desc") => allItems.OrderByDescending(x => x.TotalAmount).AsEnumerable(),
			("itemcode", "asc") => allItems.OrderBy(x => x.ReceiptItemCode ?? string.Empty).AsEnumerable(),
			("itemcode", "desc") => allItems.OrderByDescending(x => x.ReceiptItemCode ?? string.Empty).AsEnumerable(),
			("description", "desc") => allItems.OrderByDescending(x => x.Description).AsEnumerable(),
			_ => allItems.OrderBy(x => x.Description).AsEnumerable(),
		};

		List<UncategorizedItemRecord> pagedItems = sorted
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new UncategorizedItemRecord(
				x.Id,
				x.ReceiptId,
				x.ReceiptItemCode,
				x.Description,
				x.Quantity,
				x.UnitPrice,
				x.TotalAmount,
				x.Category,
				x.Subcategory,
				x.PricingMode.ToString().ToLowerInvariant()))
			.ToList();

		return new UncategorizedItemsResult(pagedItems, totalCount);
	}

	private record DescriptionSimilarityEdge(string DescA, int CountA, string DescB, int CountB, double Score);
}
