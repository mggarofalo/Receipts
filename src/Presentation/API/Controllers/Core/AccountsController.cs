using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Account.Create;
using Application.Commands.Account.Delete;
using Application.Commands.Account.Restore;
using Application.Commands.Account.Update;
using Application.Models;
using Application.Queries.Core.Account;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
[Authorize]
public class AccountsController(IMediator mediator, AccountMapper mapper, ILogger<AccountsController> logger, IEntityChangeNotifier notifier) : ControllerBase
{
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteCreateBatch = "batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "";
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get an account by ID")]
	[EndpointDescription("Returns a single account matching the provided GUID.")]
	public async Task<Results<Ok<AccountResponse>, NotFound>> GetAccountById([FromRoute] Guid id)
	{
		GetAccountByIdQuery query = new(id);
		Account? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Account {Id} not found", id);
			return TypedResults.NotFound();
		}

		AccountResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all accounts")]
	public async Task<Results<Ok<AccountListResponse>, BadRequest<string>>> GetAllAccounts([FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
	{
		if (sortBy is not null && !SortableColumns.Account.Contains(sortBy))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sortBy}'. Allowed: {string.Join(", ", SortableColumns.Account)}");
		}

		if (!SortableColumns.IsValidDirection(sortDirection))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{sortDirection}'. Allowed: asc, desc");
		}

		SortParams sort = new(sortBy, sortDirection);
		GetAllAccountsQuery query = new(offset, limit, sort);
		PagedResult<Account> result = await mediator.Send(query);

		return TypedResults.Ok(new AccountListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted accounts")]
	[EndpointDescription("Returns all accounts that have been soft-deleted.")]
	public async Task<Results<Ok<AccountListResponse>, BadRequest<string>>> GetDeletedAccounts([FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
	{
		if (sortBy is not null && !SortableColumns.Account.Contains(sortBy))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sortBy}'. Allowed: {string.Join(", ", SortableColumns.Account)}");
		}

		if (!SortableColumns.IsValidDirection(sortDirection))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{sortDirection}'. Allowed: asc, desc");
		}

		SortParams sort = new(sortBy, sortDirection);
		GetDeletedAccountsQuery query = new(offset, limit, sort);
		PagedResult<Account> result = await mediator.Send(query);

		return TypedResults.Ok(new AccountListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single account")]
	public async Task<Ok<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest model)
	{
		CreateAccountCommand command = new([mapper.ToDomain(model)]);
		List<Account> accounts = await mediator.Send(command);
		await notifier.NotifyCreated("account", accounts[0].Id);
		return TypedResults.Ok(mapper.ToResponse(accounts[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create accounts in batch")]
	public async Task<Ok<List<AccountResponse>>> CreateAccounts([FromBody] List<CreateAccountRequest> models)
	{
		CreateAccountCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Account> accounts = await mediator.Send(command);
		await notifier.NotifyBulkChanged("account", "created", accounts.Select(a => a.Id));
		return TypedResults.Ok(accounts.Select(mapper.ToResponse).ToList());
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single account")]
	public async Task<Results<NoContent, NotFound>> UpdateAccount([FromRoute] Guid id, [FromBody] UpdateAccountRequest model)
	{
		UpdateAccountCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Account {Id} not found for update", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("account", id);
		return TypedResults.NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update accounts in batch")]
	public async Task<Results<NoContent, NotFound>> UpdateAccounts([FromBody] List<UpdateAccountRequest> models)
	{
		UpdateAccountCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Accounts batch update failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("account", "updated", models.Select(m => m.Id));
		return TypedResults.NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete accounts")]
	[EndpointDescription("Deletes one or more accounts by their IDs. Returns 404 if any account is not found.")]
	public async Task<Results<NoContent, NotFound>> DeleteAccounts([FromBody] List<Guid> ids)
	{
		DeleteAccountCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Accounts delete failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("account", "deleted", ids);
		return TypedResults.NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted account")]
	[EndpointDescription("Restores a previously soft-deleted account by clearing its DeletedAt timestamp.")]
	public async Task<Results<NoContent, NotFound>> RestoreAccount([FromRoute] Guid id)
	{
		RestoreAccountCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Account {Id} not found or not deleted for restore", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("account", id);
		return TypedResults.NoContent();
	}
}
