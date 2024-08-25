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
public class AccountsController(IMediator mediator, IMapper mapper) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;

	[HttpPost]
	public async Task<ActionResult<AccountVM>> CreateAccount(AccountVM model)
	{
		CreateAccountCommand command = _mapper.Map<AccountVM, CreateAccountCommand>(model);
		Guid accountId = await _mediator.Send(command);

		model.Id = accountId;
		return Ok(model);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<AccountVM>> GetAccountById(Guid id)
	{
		GetAccountByIdQuery query = new(id);
		Account? result = await _mediator.Send(query);

		if (result == null)
		{
			return NotFound();
		}

		AccountVM model = _mapper.Map<AccountVM>(result);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<AccountVM>>> GetAllAccounts()
	{
		GetAllAccountsQuery query = new();
		List<Account> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-account-code/{accountCode}")]
	public async Task<ActionResult<List<AccountVM>>> GetAccountsByAccountCode(string accountCode)
	{
		GetAccountsByAccountCodeQuery query = new(accountCode);
		List<Account> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-name/{name}")]
	public async Task<ActionResult<List<AccountVM>>> GetAccountsByName(string name)
	{
		GetAccountsByNameQuery query = new(name);
		List<Account> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateAccount(AccountVM model)
	{
		UpdateAccountCommand command = _mapper.Map<AccountVM, UpdateAccountCommand>(model);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<ActionResult<bool>> DeleteAccount(Guid id)
	{
		DeleteAccountCommand command = new(id);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}
}