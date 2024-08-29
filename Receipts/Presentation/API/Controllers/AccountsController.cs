using Application.Commands.Account;
using Application.Queries.Account;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IMediator mediator, IMapper mapper, ILogger<AccountsController> logger) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<AccountsController> _logger = logger;

	[HttpGet("{id}")]
	public async Task<ActionResult<AccountVM>> GetAccountById(Guid id)
	{
		_logger.LogDebug("GetAccountById called with id: {Id}", id);
		GetAccountByIdQuery query = new(id);
		Account? result = await _mediator.Send(query);

		if (result == null)
		{
			_logger.LogWarning("GetAccountById called with id: {Id} not found", id);
			return NotFound();
		}

		AccountVM model = _mapper.Map<AccountVM>(result);
		_logger.LogDebug("GetAccountById called with id: {Id} found", id);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<AccountVM>>> GetAllAccounts()
	{
		_logger.LogDebug("GetAllAccounts called");
		GetAllAccountsQuery query = new();
		List<Account> result = await _mediator.Send(query);
		_logger.LogDebug("GetAllAccounts called with {Count} accounts", result.Count);

		return Ok(result);
	}

	[HttpPost]
	public async Task<ActionResult<AccountVM>> CreateAccount(List<AccountVM> models)
	{
		_logger.LogDebug("CreateAccount called with {Count} accounts", models.Count);
		CreateAccountCommand command = new(models.Select(_mapper.Map<AccountVM, Account>).ToList());
		List<Account> accounts = await _mediator.Send(command);
		return Ok(accounts.Select(_mapper.Map<Account, AccountVM>).ToList());
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateAccounts(List<AccountVM> models)
	{
		_logger.LogDebug("UpdateAccounts called with {Count} accounts", models.Count);
		UpdateAccountCommand command = new(models.Select(_mapper.Map<AccountVM, Account>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("UpdateAccounts called with {Count} accounts, but not found", models.Count);
			return NotFound();
		}

		_logger.LogDebug("UpdateAccounts called with {Count} accounts, and found", models.Count);

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteAccounts([FromBody] List<Guid> ids)
	{
		_logger.LogDebug("DeleteAccounts called with {Count} ids", ids.Count);
		DeleteAccountCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("DeleteAccounts called with {Count} ids, but not found", ids.Count);
			return NotFound();
		}

		_logger.LogDebug("DeleteAccounts called with {Count} ids, and found", ids.Count);

		return NoContent();
	}
}