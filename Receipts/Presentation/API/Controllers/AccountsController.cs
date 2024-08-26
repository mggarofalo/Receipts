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

	[HttpPost]
	public async Task<ActionResult<AccountVM>> CreateAccount(List<AccountVM> models)
	{
		CreateAccountCommand command = new(models.Select(_mapper.Map<AccountVM, Account>).ToList());
		List<Account> accounts = await _mediator.Send(command);
		return Ok(accounts.Select(_mapper.Map<Account, AccountVM>).ToList());
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateAccounts(List<AccountVM> models)
	{
		UpdateAccountCommand command = new(models.Select(_mapper.Map<AccountVM, Account>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteAccounts([FromBody] List<Guid> ids)
	{
		DeleteAccountCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}
}