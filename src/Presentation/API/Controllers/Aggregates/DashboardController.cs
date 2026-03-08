using API.Generated.Dtos;
using Application.Models.Dashboard;
using Application.Queries.Aggregates.Dashboard;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiVersion("1.0")]
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
	private static readonly DateOnly DefaultStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
	private static readonly DateOnly DefaultEndDate = DateOnly.FromDateTime(DateTime.Today);

	[HttpGet("summary")]
	[EndpointSummary("Get dashboard summary statistics")]
	[EndpointDescription("Returns high-level stats for a date range including total receipts, total spent, average trip amount, and most-used account/category.")]
	public async Task<Results<Ok<DashboardSummaryResponse>, BadRequest<string>>> GetDashboardSummary(
		[FromQuery] DateOnly? startDate,
		[FromQuery] DateOnly? endDate)
	{
		DateOnly start = startDate ?? DefaultStartDate;
		DateOnly end = endDate ?? DefaultEndDate;

		if (start > end)
		{
			return TypedResults.BadRequest("startDate must be before or equal to endDate");
		}

		GetDashboardSummaryQuery query = new(start, end);
		DashboardSummaryResult result = await mediator.Send(query);

		return TypedResults.Ok(new DashboardSummaryResponse
		{
			TotalReceipts = result.TotalReceipts,
			TotalSpent = (double)result.TotalSpent,
			AverageTripAmount = (double)result.AverageTripAmount,
			MostUsedAccount = new NameCountPair
			{
				Name = result.MostUsedAccount.Name,
				Count = result.MostUsedAccount.Count
			},
			MostUsedCategory = new NameCountPair
			{
				Name = result.MostUsedCategory.Name,
				Count = result.MostUsedCategory.Count
			}
		});
	}

	[HttpGet("spending-over-time")]
	[EndpointSummary("Get spending over time")]
	[EndpointDescription("Returns time-series spending data bucketed by the specified granularity.")]
	public async Task<Results<Ok<SpendingOverTimeResponse>, BadRequest<string>>> GetSpendingOverTime(
		[FromQuery] DateOnly? startDate,
		[FromQuery] DateOnly? endDate,
		[FromQuery] string? granularity)
	{
		DateOnly start = startDate ?? DefaultStartDate;
		DateOnly end = endDate ?? DefaultEndDate;
		string gran = granularity ?? "monthly";

		if (start > end)
		{
			return TypedResults.BadRequest("startDate must be before or equal to endDate");
		}

		string[] validGranularities = ["daily", "weekly", "monthly"];
		if (!validGranularities.Contains(gran.ToLowerInvariant()))
		{
			return TypedResults.BadRequest($"Invalid granularity '{gran}'. Allowed: daily, weekly, monthly");
		}

		GetSpendingOverTimeQuery query = new(start, end, gran);
		SpendingOverTimeResult result = await mediator.Send(query);

		return TypedResults.Ok(new SpendingOverTimeResponse
		{
			Buckets = result.Buckets.Select(b => new SpendingBucket
			{
				Period = b.Period,
				Amount = (double)b.Amount
			}).ToList()
		});
	}

	[HttpGet("spending-by-category")]
	[EndpointSummary("Get spending grouped by category")]
	[EndpointDescription("Returns spending amounts grouped by receipt item category.")]
	public async Task<Results<Ok<SpendingByCategoryResponse>, BadRequest<string>>> GetSpendingByCategory(
		[FromQuery] DateOnly? startDate,
		[FromQuery] DateOnly? endDate,
		[FromQuery] int limit = 10)
	{
		DateOnly start = startDate ?? DefaultStartDate;
		DateOnly end = endDate ?? DefaultEndDate;

		if (start > end)
		{
			return TypedResults.BadRequest("startDate must be before or equal to endDate");
		}

		if (limit <= 0 || limit > 100)
		{
			return TypedResults.BadRequest("limit must be between 1 and 100");
		}

		GetSpendingByCategoryQuery query = new(start, end, limit);
		SpendingByCategoryResult result = await mediator.Send(query);

		return TypedResults.Ok(new SpendingByCategoryResponse
		{
			Items = result.Items.Select(i => new SpendingCategoryItem
			{
				CategoryName = i.CategoryName,
				Amount = (double)i.Amount,
				Percentage = (double)i.Percentage
			}).ToList()
		});
	}

	[HttpGet("spending-by-account")]
	[EndpointSummary("Get spending grouped by account")]
	[EndpointDescription("Returns spending amounts grouped by payment account.")]
	public async Task<Results<Ok<SpendingByAccountResponse>, BadRequest<string>>> GetSpendingByAccount(
		[FromQuery] DateOnly? startDate,
		[FromQuery] DateOnly? endDate)
	{
		DateOnly start = startDate ?? DefaultStartDate;
		DateOnly end = endDate ?? DefaultEndDate;

		if (start > end)
		{
			return TypedResults.BadRequest("startDate must be before or equal to endDate");
		}

		GetSpendingByAccountQuery query = new(start, end);
		SpendingByAccountResult result = await mediator.Send(query);

		return TypedResults.Ok(new SpendingByAccountResponse
		{
			Items = result.Items.Select(i => new SpendingAccountItem
			{
				AccountId = i.AccountId,
				AccountName = i.AccountName,
				Amount = (double)i.Amount,
				Percentage = (double)i.Percentage
			}).ToList()
		});
	}
}
