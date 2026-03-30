using API.Generated.Dtos;
using Application.Commands.Reports;
using Application.Queries.Aggregates.Reports;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using AppReports = Application.Models.Reports;

namespace API.Controllers.Aggregates;

[ApiVersion("1.0")]
[ApiController]
[Route("api/reports")]
[Produces("application/json")]
[Authorize]
public class ReportsController(IMediator mediator) : ControllerBase
{
	[HttpGet("out-of-balance")]
	[EndpointSummary("Get out-of-balance receipts report")]
	[EndpointDescription("Returns all receipts where item subtotal + tax + adjustments does not equal the transaction total.")]
	public async Task<Results<Ok<OutOfBalanceResponse>, BadRequest<string>>> GetOutOfBalance(
		[FromQuery] string? sortBy,
		[FromQuery] string? sortDirection,
		[FromQuery] int? page,
		[FromQuery] int? pageSize,
		CancellationToken cancellationToken)
	{
		string sort = sortBy ?? "date";
		string direction = sortDirection ?? "asc";
		int pg = page ?? 1;
		int ps = pageSize ?? 50;

		string[] validSortColumns = ["date", "difference"];
		if (!validSortColumns.Contains(sort.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sort}'. Allowed: date, difference");
		}

		string[] validDirections = ["asc", "desc"];
		if (!validDirections.Contains(direction.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{direction}'. Allowed: asc, desc");
		}

		if (pg < 1)
		{
			return TypedResults.BadRequest("page must be at least 1");
		}

		if (ps < 1 || ps > 100)
		{
			return TypedResults.BadRequest("pageSize must be between 1 and 100");
		}

		GetOutOfBalanceReportQuery query = new(sort, direction, pg, ps);
		AppReports.OutOfBalanceResult result = await mediator.Send(query, cancellationToken);

		return TypedResults.Ok(new OutOfBalanceResponse
		{
			TotalCount = result.TotalCount,
			TotalDiscrepancy = (double)result.TotalDiscrepancy,
			Items = result.Items.Select(i => new OutOfBalanceItem
			{
				ReceiptId = i.ReceiptId,
				Location = i.Location,
				Date = i.Date,
				ItemSubtotal = (double)i.ItemSubtotal,
				TaxAmount = (double)i.TaxAmount,
				AdjustmentTotal = (double)i.AdjustmentTotal,
				ExpectedTotal = (double)i.ExpectedTotal,
				TransactionTotal = (double)i.TransactionTotal,
				Difference = (double)i.Difference
			}).ToList()
		});
	}

	[HttpGet("spending-by-location")]
	[EndpointSummary("Get spending by location report")]
	[EndpointDescription("Returns spending aggregated by store location with visit count, total, and average per visit.")]
	public async Task<Results<Ok<SpendingByLocationResponse>, BadRequest<string>>> GetSpendingByLocation(
		[FromQuery] DateOnly? startDate,
		[FromQuery] DateOnly? endDate,
		[FromQuery] string? sortBy,
		[FromQuery] string? sortDirection,
		[FromQuery] int? page,
		[FromQuery] int? pageSize,
		CancellationToken cancellationToken)
	{
		string sort = sortBy ?? "total";
		string direction = sortDirection ?? "desc";
		int pg = page ?? 1;
		int ps = pageSize ?? 50;

		string[] validSortColumns = ["location", "visits", "total", "averagepervisit"];
		if (!validSortColumns.Contains(sort.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sort}'. Allowed: location, visits, total, averagePerVisit");
		}

		string[] validDirections = ["asc", "desc"];
		if (!validDirections.Contains(direction.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{direction}'. Allowed: asc, desc");
		}

		if (pg < 1)
		{
			return TypedResults.BadRequest("page must be at least 1");
		}

		if (ps < 1 || ps > 100)
		{
			return TypedResults.BadRequest("pageSize must be between 1 and 100");
		}

		if (startDate.HasValue && endDate.HasValue && startDate > endDate)
		{
			return TypedResults.BadRequest("startDate must be before or equal to endDate");
		}

		GetSpendingByLocationReportQuery query = new(startDate, endDate, sort, direction, pg, ps);
		AppReports.SpendingByLocationResult result = await mediator.Send(query, cancellationToken);

		return TypedResults.Ok(new SpendingByLocationResponse
		{
			TotalCount = result.TotalCount,
			GrandTotal = (double)result.GrandTotal,
			Items = result.Items.Select(i => new SpendingByLocationItem
			{
				Location = i.Location,
				Visits = i.Visits,
				Total = (double)i.Total,
				AveragePerVisit = (double)i.AveragePerVisit
			}).ToList()
		});
	}

	[HttpGet("item-similarity")]
	[EndpointSummary("Get item similarity clusters")]
	[EndpointDescription("Returns groups of receipt items with similar descriptions using trigram matching.")]
	public async Task<Results<Ok<ItemSimilarityResponse>, BadRequest<string>>> GetItemSimilarity(
		[FromQuery] double? threshold,
		[FromQuery] string? sortBy,
		[FromQuery] string? sortDirection,
		[FromQuery] int? page,
		[FromQuery] int? pageSize,
		CancellationToken cancellationToken)
	{
		double th = threshold ?? 0.7;
		string sort = sortBy ?? "occurrences";
		string direction = sortDirection ?? "desc";
		int pg = page ?? 1;
		int ps = pageSize ?? 50;

		if (th < 0.3 || th > 0.95)
		{
			return TypedResults.BadRequest("threshold must be between 0.3 and 0.95");
		}

		string[] validSortColumns = ["canonicalName", "occurrences", "maxSimilarity"];
		if (!validSortColumns.Contains(sort, StringComparer.OrdinalIgnoreCase))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sort}'. Allowed: canonicalName, occurrences, maxSimilarity");
		}

		string[] validDirections = ["asc", "desc"];
		if (!validDirections.Contains(direction.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{direction}'. Allowed: asc, desc");
		}

		if (pg < 1)
		{
			return TypedResults.BadRequest("page must be at least 1");
		}

		if (ps < 1 || ps > 100)
		{
			return TypedResults.BadRequest("pageSize must be between 1 and 100");
		}

		GetItemSimilarityReportQuery query = new(th, sort, direction, pg, ps);
		AppReports.ItemSimilarityResult result = await mediator.Send(query, cancellationToken);

		return TypedResults.Ok(new ItemSimilarityResponse
		{
			TotalCount = result.TotalCount,
			Groups = result.Groups.Select(g => new ItemSimilarityGroup
			{
				CanonicalName = g.CanonicalName,
				Variants = g.Variants,
				ItemIds = g.ItemIds,
				Occurrences = g.Occurrences,
				MaxSimilarity = g.MaxSimilarity
			}).ToList()
		});
	}

	[HttpPost("item-similarity/rename")]
	[EndpointSummary("Rename all items in a similarity group")]
	[EndpointDescription("Updates the description of all specified receipt items to the given canonical name.")]
	public async Task<Results<Ok<ItemSimilarityRenameResponse>, BadRequest<string>>> RenameItemSimilarityGroup(
		[FromBody] ItemSimilarityRenameRequest request,
		CancellationToken cancellationToken)
	{
		if (request.ItemIds.Count == 0)
		{
			return TypedResults.BadRequest("itemIds must not be empty");
		}

		if (string.IsNullOrWhiteSpace(request.NewDescription))
		{
			return TypedResults.BadRequest("newDescription must not be empty");
		}

		RenameItemSimilarityGroupCommand command = new(request.ItemIds.ToList(), request.NewDescription);
		int updatedCount = await mediator.Send(command, cancellationToken);

		return TypedResults.Ok(new ItemSimilarityRenameResponse
		{
			UpdatedCount = updatedCount
		});
	}

	[HttpGet("item-descriptions")]
	[EndpointSummary("Search item descriptions for autocomplete")]
	[EndpointDescription("Returns distinct item descriptions with their category and occurrence count, filtered by a search term (minimum 2 characters).")]
	public async Task<Results<Ok<ItemDescriptionsResponse>, BadRequest<string>>> GetItemDescriptions(
		[FromQuery] string? search,
		[FromQuery] bool? categoryOnly,
		[FromQuery] int? limit,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(search) || search.Length < 2)
		{
			return TypedResults.BadRequest("search must be at least 2 characters");
		}

		int lim = limit ?? 20;
		if (lim < 1 || lim > 50)
		{
			return TypedResults.BadRequest("limit must be between 1 and 50");
		}

		GetItemDescriptionsQuery query = new(search, categoryOnly ?? false, lim);
		AppReports.ItemDescriptionResult result = await mediator.Send(query, cancellationToken);

		return TypedResults.Ok(new ItemDescriptionsResponse
		{
			Items = result.Items.Select(i => new ItemDescriptionItem
			{
				Description = i.Description,
				Category = i.Category,
				Occurrences = i.Occurrences
			}).ToList()
		});
	}

	[HttpGet("item-cost-over-time")]
	[EndpointSummary("Get item cost over time")]
	[EndpointDescription("Returns time-series cost data for a specific item description or category.")]
	public async Task<Results<Ok<ItemCostOverTimeResponse>, BadRequest<string>>> GetItemCostOverTime(
		[FromQuery] string? description,
		[FromQuery] string? category,
		[FromQuery] DateOnly? startDate,
		[FromQuery] DateOnly? endDate,
		[FromQuery] string? granularity,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(description) && string.IsNullOrEmpty(category))
		{
			return TypedResults.BadRequest("Either description or category is required");
		}

		string gran = granularity ?? "exact";
		string[] validGranularities = ["exact", "monthly", "yearly"];
		if (!validGranularities.Contains(gran.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid granularity '{gran}'. Allowed: exact, monthly, yearly");
		}

		if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
		{
			return TypedResults.BadRequest("startDate must be before or equal to endDate");
		}

		GetItemCostOverTimeQuery query = new(description, category, startDate, endDate, gran);
		AppReports.ItemCostOverTimeResult result = await mediator.Send(query, cancellationToken);

		return TypedResults.Ok(new ItemCostOverTimeResponse
		{
			Buckets = result.Buckets.Select(b => new ItemCostBucket
			{
				Period = b.Period,
				Amount = (double)b.Amount
			}).ToList()
		});
	}

	[HttpGet("duplicates")]
	[EndpointSummary("Get duplicate receipt detection report")]
	[EndpointDescription("Finds receipts that share the same date+location, date+total, or all three fields and may be double entries.")]
	public async Task<Results<Ok<DuplicatesResponse>, BadRequest<string>>> GetDuplicates(
		[FromQuery] string? matchOn,
		[FromQuery] string? locationTolerance,
		[FromQuery] double? totalTolerance,
		CancellationToken cancellationToken)
	{
		string match = matchOn ?? "DateAndLocation";
		string locTol = locationTolerance ?? "exact";
		decimal totTol = (decimal)(totalTolerance ?? 0);

		string[] validMatchOn = ["DateAndLocation", "DateAndTotal", "DateAndLocationAndTotal"];
		if (!validMatchOn.Contains(match))
		{
			return TypedResults.BadRequest($"Invalid matchOn '{match}'. Allowed: DateAndLocation, DateAndTotal, DateAndLocationAndTotal");
		}

		string[] validLocTolerance = ["exact", "normalized"];
		if (!validLocTolerance.Contains(locTol.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid locationTolerance '{locTol}'. Allowed: exact, normalized");
		}

		if (totTol < 0)
		{
			return TypedResults.BadRequest("totalTolerance must be >= 0");
		}

		GetDuplicateDetectionReportQuery query = new(match, locTol, totTol);
		AppReports.DuplicateDetectionResult result = await mediator.Send(query, cancellationToken);

		return TypedResults.Ok(new DuplicatesResponse
		{
			GroupCount = result.GroupCount,
			TotalDuplicateReceipts = result.TotalDuplicateReceipts,
			Groups = result.Groups.Select(g => new DuplicateGroup
			{
				MatchKey = g.MatchKey,
				Receipts = g.Receipts.Select(r => new DuplicateReceipt
				{
					ReceiptId = r.ReceiptId,
					Location = r.Location,
					Date = r.Date,
					TransactionTotal = (double)r.TransactionTotal
				}).ToList()
			}).ToList()
		});
	}
}
