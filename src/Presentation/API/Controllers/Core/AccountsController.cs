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
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
	[ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<AccountResponse>> GetAccountById([FromRoute] Guid id)
	{
		GetAccountByIdQuery query = new(id);
		Account? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Account {Id} not found", id);
			return NotFound();
		}

		AccountResponse model = mapper.ToResponse(result);
		return Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all accounts")]
	[ProducesResponseType<AccountListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<AccountListResponse>> GetAllAccounts([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetAllAccountsQuery query = new(offset, limit);
		PagedResult<Account> result = await mediator.Send(query);

		return Ok(new AccountListResponse
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
	[ProducesResponseType<AccountListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<AccountListResponse>> GetDeletedAccounts([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedAccountsQuery query = new(offset, limit);
		PagedResult<Account> result = await mediator.Send(query);

		return Ok(new AccountListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single account")]
	[ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest model)
	{
		CreateAccountCommand command = new([mapper.ToDomain(model)]);
		List<Account> accounts = await mediator.Send(command);
		await notifier.NotifyCreated("account", accounts[0].Id);
		return Ok(mapper.ToResponse(accounts[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create accounts in batch")]
	[ProducesResponseType<List<AccountResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<AccountResponse>>> CreateAccounts([FromBody] List<CreateAccountRequest> models)
	{
		CreateAccountCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Account> accounts = await mediator.Send(command);
		await notifier.NotifyBulkChanged("account", "created", accounts.Select(a => a.Id));
		return Ok(accounts.Select(mapper.ToResponse).ToList());
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single account")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateAccount([FromRoute] Guid id, [FromBody] UpdateAccountRequest model)
	{
		UpdateAccountCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Account {Id} not found for update", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("account", id);
		return NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update accounts in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateAccounts([FromBody] List<UpdateAccountRequest> models)
	{
		UpdateAccountCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Accounts batch update failed — not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("account", "updated", models.Select(m => m.Id));
		return NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete accounts")]
	[EndpointDescription("Deletes one or more accounts by their IDs. Returns 404 if any account is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> DeleteAccounts([FromBody] List<Guid> ids)
	{
		DeleteAccountCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Accounts delete failed — not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("account", "deleted", ids);
		return NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted account")]
	[EndpointDescription("Restores a previously soft-deleted account by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RestoreAccount([FromRoute] Guid id)
	{
		RestoreAccountCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Account {Id} not found or not deleted for restore", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("account", id);
		return NoContent();
	}
}
