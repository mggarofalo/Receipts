using Application.Commands.Transaction;
using Application.Queries.Transaction;
using AutoMapper;
using Domain;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMediator mediator, IMapper mapper) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;

	[HttpPost]
	public async Task<ActionResult<TransactionVM>> CreateTransaction(List<TransactionVM> models)
	{
		CreateTransactionCommand command = new(models.Select(_mapper.Map<TransactionVM, Transaction>).ToList());
		List<Transaction> transactions = await _mediator.Send(command);
		return Ok(transactions.Select(_mapper.Map<Transaction, TransactionVM>).ToList());
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<TransactionVM>> GetTransactionById(Guid id)
	{
		GetTransactionByIdQuery query = new(id);
		Transaction? result = await _mediator.Send(query);

		if (result == null)
		{
			return NotFound();
		}

		TransactionVM model = _mapper.Map<TransactionVM>(result);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<TransactionVM>>> GetAllTransactions()
	{
		GetAllTransactionsQuery query = new();
		List<Transaction> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-date")]
	public async Task<ActionResult<List<TransactionVM>>> GetTransactionsByDateRange(DateOnly startDate, DateOnly endDate)
	{
		GetTransactionsByDateRangeQuery query = new(startDate, endDate);
		List<Transaction> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-money")]
	public async Task<ActionResult<List<TransactionVM>>> GetTransactionsByMoneyRange(decimal minAmount, decimal maxAmount)
	{
		GetTransactionsByMoneyRangeQuery query = new(new Money(minAmount), new Money(maxAmount));
		List<Transaction> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateTransactions(List<TransactionVM> models)
	{
		UpdateTransactionCommand command = new(models.Select(_mapper.Map<TransactionVM, Transaction>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteTransactions([FromBody] List<Guid> ids)
	{
		DeleteTransactionCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}
}