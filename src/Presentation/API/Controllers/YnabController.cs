using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Ynab.AccountMapping;
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

	[HttpGet("accounts")]
	[EndpointSummary("List YNAB accounts")]
	[EndpointDescription("Returns the list of open/active accounts from the selected YNAB budget.")]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<Results<Ok<YnabAccountListResponse>, StatusCodeHttpResult>> GetAccounts(CancellationToken cancellationToken)
	{
		if (!ynabClient.IsConfigured)
		{
			logger.LogWarning("YNAB API client is not configured — missing personal access token");
			return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
		}

		try
		{
			YnabBudgetSelection selection = await mediator.Send(new GetSelectedYnabBudgetQuery(), cancellationToken);
			if (string.IsNullOrEmpty(selection.SelectedBudgetId))
			{
				return TypedResults.Ok(mapper.ToAccountListResponse([]));
			}

			List<YnabAccount> accounts = await ynabClient.GetAccountsAsync(selection.SelectedBudgetId, cancellationToken);

			// Return only open (not closed) accounts
			List<YnabAccount> activeAccounts = accounts.Where(a => !a.Closed).ToList();
			return TypedResults.Ok(mapper.ToAccountListResponse(activeAccounts));
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

	[HttpGet("account-mappings")]
	[EndpointSummary("List YNAB account mappings")]
	[EndpointDescription("Returns all mappings between receipts accounts and YNAB accounts.")]
	public async Task<Ok<YnabAccountMappingListResponse>> GetAccountMappings(CancellationToken cancellationToken)
	{
		List<YnabAccountMappingDto> mappings = await mediator.Send(new GetYnabAccountMappingsQuery(), cancellationToken);
		return TypedResults.Ok(mapper.ToAccountMappingListResponse(mappings));
	}

	[HttpPost("account-mappings")]
	[EndpointSummary("Create a YNAB account mapping")]
	[EndpointDescription("Maps a receipts account to a YNAB account.")]
	public async Task<Results<Created<YnabAccountMappingResponse>, BadRequest<string>>> CreateAccountMapping(
		[FromBody] CreateYnabAccountMappingRequest request,
		CancellationToken cancellationToken)
	{
		try
		{
			YnabAccountMappingDto mapping = await mediator.Send(
				new CreateYnabAccountMappingCommand(
					request.ReceiptsAccountId,
					request.YnabAccountId,
					request.YnabAccountName,
					request.YnabBudgetId),
				cancellationToken);

			YnabAccountMappingResponse response = mapper.ToAccountMappingResponse(mapping);
			return TypedResults.Created($"/api/ynab/account-mappings/{response.Id}", response);
		}
		catch (InvalidOperationException ex)
		{
			return TypedResults.BadRequest(ex.Message);
		}
	}

	[HttpPut("account-mappings/{id}")]
	[EndpointSummary("Update a YNAB account mapping")]
	[EndpointDescription("Updates the YNAB account for an existing mapping.")]
	public async Task<Results<NoContent, NotFound>> UpdateAccountMapping(
		[FromRoute] Guid id,
		[FromBody] UpdateYnabAccountMappingRequest request,
		CancellationToken cancellationToken)
	{
		YnabAccountMappingDto? existing = await mediator.Send(new GetYnabAccountMappingByIdQuery(id), cancellationToken);
		if (existing is null)
		{
			return TypedResults.NotFound();
		}

		await mediator.Send(
			new UpdateYnabAccountMappingCommand(
				id,
				request.YnabAccountId,
				request.YnabAccountName,
				request.YnabBudgetId),
			cancellationToken);

		return TypedResults.NoContent();
	}

	[HttpDelete("account-mappings/{id}")]
	[EndpointSummary("Delete a YNAB account mapping")]
	[EndpointDescription("Removes a mapping between a receipts account and a YNAB account.")]
	public async Task<Results<NoContent, NotFound>> DeleteAccountMapping(
		[FromRoute] Guid id,
		CancellationToken cancellationToken)
	{
		YnabAccountMappingDto? existing = await mediator.Send(new GetYnabAccountMappingByIdQuery(id), cancellationToken);
		if (existing is null)
		{
			return TypedResults.NotFound();
		}

		await mediator.Send(new DeleteYnabAccountMappingCommand(id), cancellationToken);
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
