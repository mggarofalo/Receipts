using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Account.Create;
using Application.Commands.Account.Delete;
using Application.Commands.Account.Update;
using Application.Queries.Core.Account;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
public class AccountsController(IMediator mediator, AccountMapper mapper, ILogger<AccountsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteCreateBatch = "batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get an account by ID")]
	[EndpointDescription("Returns a single account matching the provided GUID.")]
	[ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AccountResponse>> GetAccountById([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("GetAccountById called with id: {Id}", id);
			GetAccountByIdQuery query = new(id);
			Account? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetAccountById called with id: {Id} not found", id);
				return NotFound();
			}

			AccountResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetAccountById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetAccountById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all accounts")]
	[ProducesResponseType<List<AccountResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<AccountResponse>>> GetAllAccounts()
	{
		try
		{
			logger.LogDebug("GetAllAccounts called");
			GetAllAccountsQuery query = new();
			List<Account> result = await mediator.Send(query);
			logger.LogDebug("GetAllAccounts called with {Count} accounts", result.Count);

			List<AccountResponse> model = result.Select(mapper.ToResponse).ToList();
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllAccounts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single account")]
	[ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest model)
	{
		try
		{
			logger.LogDebug("CreateAccount called");
			CreateAccountCommand command = new([mapper.ToDomain(model)]);
			List<Account> accounts = await mediator.Send(command);
			return Ok(mapper.ToResponse(accounts[0]));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateAccount));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create accounts in batch")]
	[ProducesResponseType<List<AccountResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<AccountResponse>>> CreateAccounts([FromBody] List<CreateAccountRequest> models)
	{
		try
		{
			logger.LogDebug("CreateAccounts called with {Count} accounts", models.Count);
			CreateAccountCommand command = new(models.Select(mapper.ToDomain).ToList());
			List<Account> accounts = await mediator.Send(command);
			return Ok(accounts.Select(mapper.ToResponse).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateAccounts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single account")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateAccount([FromRoute] Guid id, [FromBody] UpdateAccountRequest model)
	{
		try
		{
			logger.LogDebug("UpdateAccount called for id: {Id}", id);
			UpdateAccountCommand command = new([mapper.ToDomain(model)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateAccount called for id: {Id}, but not found", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(UpdateAccount), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update accounts in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateAccounts([FromBody] List<UpdateAccountRequest> models)
	{
		try
		{
			logger.LogDebug("UpdateAccounts called with {Count} accounts", models.Count);
			UpdateAccountCommand command = new(models.Select(mapper.ToDomain).ToList());
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateAccounts called with {Count} accounts, but not found", models.Count);
				return NotFound();
			}

			logger.LogDebug("UpdateAccounts called with {Count} accounts, and found", models.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateAccounts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete accounts")]
	[EndpointDescription("Deletes one or more accounts by their IDs. Returns 404 if any account is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> DeleteAccounts([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteAccounts called with {Count} ids", ids.Count);
			DeleteAccountCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteAccounts called with {Count} ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteAccounts called with {Count} ids, and found", ids.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteAccounts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
