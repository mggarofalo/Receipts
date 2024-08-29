using Application.Commands.Transaction;
using Application.Queries.Transaction;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMediator mediator, IMapper mapper, ILogger<TransactionsController> logger) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<TransactionsController> _logger = logger;

	[HttpGet("{id}")]
	public async Task<ActionResult<TransactionVM>> GetTransactionById(Guid id)
	{
		_logger.LogDebug("GetTransactionById called with id: {Id}", id);
		GetTransactionByIdQuery query = new(id);
		Transaction? result = await _mediator.Send(query);

		if (result == null)
		{
			_logger.LogWarning("GetTransactionById called with id: {Id} not found", id);
			return NotFound();
		}

		TransactionVM model = _mapper.Map<TransactionVM>(result);
		_logger.LogDebug("GetTransactionById called with id: {Id} found", id);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<TransactionVM>>> GetAllTransactions()
	{
		_logger.LogDebug("GetAllTransactions called");
		GetAllTransactionsQuery query = new();
		List<Transaction> result = await _mediator.Send(query);
		_logger.LogDebug("GetAllTransactions called with {Count} transactions", result.Count);
		return Ok(result);
	}

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<List<TransactionVM>>> GetTransactionsByReceiptId(Guid receiptId)
	{
		_logger.LogDebug("GetTransactionsByReceiptId called with receiptId: {ReceiptId}", receiptId);
		GetTransactionsByReceiptIdQuery query = new(receiptId);
		List<Transaction> result = await _mediator.Send(query);
		_logger.LogDebug("GetTransactionsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
		return Ok(result);
	}

	[HttpPost]
	public async Task<ActionResult<TransactionVM>> CreateTransaction(List<TransactionVM> models)
	{
		_logger.LogDebug("CreateTransaction called with {Count} transactions", models.Count);
		CreateTransactionCommand command = new(models.Select(_mapper.Map<TransactionVM, Transaction>).ToList());
		List<Transaction> transactions = await _mediator.Send(command);
		_logger.LogDebug("CreateTransaction called with {Count} transactions, and created", models.Count);
		return Ok(transactions.Select(_mapper.Map<Transaction, TransactionVM>).ToList());
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateTransactions(List<TransactionVM> models)
	{
		_logger.LogDebug("UpdateTransactions called with {Count} transactions", models.Count);
		UpdateTransactionCommand command = new(models.Select(_mapper.Map<TransactionVM, Transaction>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("UpdateTransactions called with {Count} transactions, but not found", models.Count);
			return NotFound();
		}

		_logger.LogDebug("UpdateTransactions called with {Count} transactions, and found", models.Count);

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteTransactions([FromBody] List<Guid> ids)
	{
		_logger.LogDebug("DeleteTransactions called with {Count} transaction ids", ids.Count);
		DeleteTransactionCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("DeleteTransactions called with {Count} transaction ids, but not found", ids.Count);
			return NotFound();
		}

		_logger.LogDebug("DeleteTransactions called with {Count} transaction ids, and found", ids.Count);

		return NoContent();
	}
}