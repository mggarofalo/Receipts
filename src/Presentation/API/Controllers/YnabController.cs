using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Ynab.SelectBudget;
using Application.Interfaces.Services;
using Application.Models.Ynab;
using Application.Queries.Core.Ynab;
using Asp.Versioning;
using Common;
using Infrastructure.Ynab;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/ynab")]
[Produces("application/json")]
[Authorize]
public class YnabController(IMediator mediator, IYnabApiClient ynabClient, YnabMapper mapper, ILogger<YnabController> logger) : ControllerBase
{
	[HttpGet("budgets")]
	[EndpointSummary("List YNAB budgets")]
	[EndpointDescription("Returns the list of budgets from the connected YNAB account.")]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<Results<Ok<YnabBudgetListResponse>, StatusCodeHttpResult>> GetBudgets(CancellationToken cancellationToken)
	{
		if (!ynabClient.IsConfigured)
		{
			logger.LogWarning("YNAB API client is not configured — missing personal access token");
			return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
		}

		try
		{
			List<YnabBudget> budgets = await mediator.Send(new GetYnabBudgetsQuery(), cancellationToken);
			return TypedResults.Ok(mapper.ToBudgetListResponse(budgets));
		}
		catch (YnabAuthException ex)
		{
			logger.LogWarning(ex, "YNAB authentication failed");
			return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
		}
	}

	[HttpGet("settings/budget")]
	[EndpointSummary("Get the selected YNAB budget")]
	[EndpointDescription("Returns the currently selected YNAB budget ID.")]
	public async Task<Ok<YnabBudgetSettingsResponse>> GetBudgetSettings(CancellationToken cancellationToken)
	{
		YnabBudgetSelection selection = await mediator.Send(new GetSelectedYnabBudgetQuery(), cancellationToken);
		return TypedResults.Ok(mapper.ToBudgetSettingsResponse(selection));
	}

	[HttpPut("settings/budget")]
	[EndpointSummary("Select a YNAB budget")]
	[EndpointDescription("Sets the active YNAB budget for sync operations.")]
	public async Task<NoContent> SelectBudget([FromBody] SelectYnabBudgetRequest request, CancellationToken cancellationToken)
	{
		await mediator.Send(new SelectYnabBudgetCommand(request.BudgetId), cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpGet("sync-status/{transactionId}")]
	[EndpointSummary("Get YNAB sync status for a transaction")]
	[EndpointDescription("Returns the sync record for a given local transaction ID and sync type.")]
	public async Task<Results<Ok<YnabSyncRecordResponse>, NotFound>> GetSyncStatus(
		[FromRoute] Guid transactionId,
		[FromQuery] YnabSyncType syncType,
		CancellationToken cancellationToken)
	{
		YnabSyncRecordDto? record = await mediator.Send(new GetYnabSyncRecordByTransactionQuery(transactionId, syncType), cancellationToken);

		if (record is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(mapper.ToSyncRecordResponse(record));
	}
}
