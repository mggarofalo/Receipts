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
public class AccountsController(IMediator mediator, IMapper mapper, ILogger<AccountsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	[HttpGet("{id}")]
	public async Task<ActionResult<AccountVM>> GetAccountById(Guid id)
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

	[HttpGet]
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

	[HttpPost]
	public async Task<ActionResult<AccountVM>> CreateAccounts(List<AccountVM> models)
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

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateAccounts(List<AccountVM> models)
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

	[HttpDelete]
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