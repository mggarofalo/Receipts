using Application.Commands.Account;
using Application.Queries.Core.Account;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController(IMediator mediator, IMapper mapper, ILogger<AccountsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteUpdate = "";
	public const string RouteDelete = "";

	/// <summary>
	/// Get an account by its ID
	/// </summary>
	/// <param name="id">The ID of the account</param>
	/// <returns>The account</returns>
	/// <response code="200">Returns the account</response>
	/// <response code="404">If the account is not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetById)]
	[ProducesResponseType(typeof(AccountVM), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AccountVM>> GetAccountById([FromRoute] Guid id)
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

			AccountVM model = mapper.Map<Account, AccountVM>(result);
			logger.LogDebug("GetAccountById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetAccountById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Get all accounts
	/// </summary>
	/// <returns>A list of all accounts</returns>
	/// <response code="200">Returns the list of accounts</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetAll)]
	[ProducesResponseType(typeof(List<AccountVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<AccountVM>>> GetAllAccounts()
	{
		try
		{
			logger.LogDebug("GetAllAccounts called");
			GetAllAccountsQuery query = new();
			List<Account> result = await mediator.Send(query);
			logger.LogDebug("GetAllAccounts called with {Count} accounts", result.Count);

			List<AccountVM> model = result.Select(mapper.Map<Account, AccountVM>).ToList();
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllAccounts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Create new accounts
	/// </summary>
	/// <param name="models">The accounts to create</param>
	/// <returns>The created accounts</returns>
	/// <response code="200">Returns the created accounts</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPost(RouteCreate)]
	[ProducesResponseType(typeof(List<AccountVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<AccountVM>>> CreateAccounts([FromBody] List<AccountVM> models)
	{
		try
		{
			logger.LogDebug("CreateAccount called with {Count} accounts", models.Count);
			CreateAccountCommand command = new(models.Select(mapper.Map<AccountVM, Account>).ToList());
			List<Account> accounts = await mediator.Send(command);
			return Ok(accounts.Select(mapper.Map<Account, AccountVM>).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateAccounts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Update existing accounts
	/// </summary>
	/// <param name="models">The accounts to update</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the accounts were successfully updated</response>
	/// <response code="404">If the accounts were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPut(RouteUpdate)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateAccounts([FromBody] List<AccountVM> models)
	{
		try
		{
			logger.LogDebug("UpdateAccounts called with {Count} accounts", models.Count);
			UpdateAccountCommand command = new(models.Select(mapper.Map<AccountVM, Account>).ToList());
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

	/// <summary>
	/// Delete accounts
	/// </summary>
	/// <param name="ids">The IDs of the accounts to delete</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the accounts were successfully deleted</response>
	/// <response code="404">If the accounts were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpDelete(RouteDelete)]
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