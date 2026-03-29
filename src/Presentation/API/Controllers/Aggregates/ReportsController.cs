using API.Generated.Dtos;
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
}
