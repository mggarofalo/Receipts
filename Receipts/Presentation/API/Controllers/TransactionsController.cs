using Application.Commands.Transaction;
using Application.Queries.Transaction;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMediator mediator, IMapper mapper, ILogger<TransactionsController> logger) : ControllerBase
{
	[HttpGet("{id}")]
	public async Task<ActionResult<TransactionVM>> GetTransactionById(Guid id)
	{
		try
		{
			logger.LogDebug("GetTransactionById called with id: {Id}", id);
			GetTransactionByIdQuery query = new(id);
			Transaction? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetTransactionById called with id: {Id} not found", id);
				return NotFound();
			}

			TransactionVM model = mapper.Map<TransactionVM>(result);
			logger.LogDebug("GetTransactionById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in GetTransactionById for id: {Id}", id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet]
	public async Task<ActionResult<List<TransactionVM>>> GetAllTransactions()
	{
		try
		{
			logger.LogDebug("GetAllTransactions called");
			GetAllTransactionsQuery query = new();
			List<Transaction> result = await mediator.Send(query);
			logger.LogDebug("GetAllTransactions called with {Count} transactions", result.Count);
			return Ok(result);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in GetAllTransactions");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<List<TransactionVM>>> GetTransactionsByReceiptId(Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetTransactionsByReceiptId called with receiptId: {ReceiptId}", receiptId);
			GetTransactionsByReceiptIdQuery query = new(receiptId);
			List<Transaction> result = await mediator.Send(query);
			logger.LogDebug("GetTransactionsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
			return Ok(result);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in GetTransactionsByReceiptId for receiptId: {ReceiptId}", receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost]
	public async Task<ActionResult<TransactionVM>> CreateTransaction(List<TransactionVM> models)
	{
		try
		{
			logger.LogDebug("CreateTransaction called with {Count} transactions", models.Count);
			CreateTransactionCommand command = new(models.Select(mapper.Map<TransactionVM, Transaction>).ToList());
			List<Transaction> transactions = await mediator.Send(command);
			logger.LogDebug("CreateTransaction called with {Count} transactions, and created", models.Count);
			return Ok(transactions.Select(mapper.Map<Transaction, TransactionVM>).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in CreateTransaction");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateTransactions(List<TransactionVM> models)
	{
		try
		{
			logger.LogDebug("UpdateTransactions called with {Count} transactions", models.Count);
			UpdateTransactionCommand command = new(models.Select(mapper.Map<TransactionVM, Transaction>).ToList());
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateTransactions called with {Count} transactions, but not found", models.Count);
				return NotFound();
			}

			logger.LogDebug("UpdateTransactions called with {Count} transactions, and found", models.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in UpdateTransactions");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteTransactions([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteTransactions called with {Count} transaction ids", ids.Count);
			DeleteTransactionCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteTransactions called with {Count} transaction ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteTransactions called with {Count} transaction ids, and found", ids.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in DeleteTransactions");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}