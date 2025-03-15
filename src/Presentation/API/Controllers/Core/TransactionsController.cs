using Application.Commands.Transaction.Create;
using Application.Commands.Transaction.Update;
using Application.Commands.Transaction.Delete;
using Application.Queries.Core.Transaction;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController(IMediator mediator, IMapper mapper, ILogger<TransactionsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteGetByReceiptId = "by-receipt-id/{receiptId}";
	public const string RouteCreate = "{receiptId}/{accountId}";
	public const string RouteUpdate = "{receiptId}/{accountId}";
	public const string RouteDelete = "";

	/// <summary>
	/// Get a transaction by its ID
	/// </summary>
	/// <param name="id">The ID of the transaction</param>
	/// <returns>The transaction</returns>
	/// <response code="200">Returns the transaction</response>
	/// <response code="404">If the transaction is not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetById)]
	[ProducesResponseType(typeof(TransactionVM), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TransactionVM>> GetTransactionById([FromRoute] Guid id)
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

			TransactionVM model = mapper.Map<Transaction, TransactionVM>(result);
			logger.LogDebug("GetTransactionById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTransactionById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Get all transactions
	/// </summary>
	/// <returns>A list of all transactions</returns>
	/// <response code="200">Returns the list of transactions</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetAll)]
	[ProducesResponseType(typeof(List<TransactionVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionVM>>> GetAllTransactions()
	{
		try
		{
			logger.LogDebug("GetAllTransactions called");
			GetAllTransactionsQuery query = new();
			List<Transaction> result = await mediator.Send(query);
			logger.LogDebug("GetAllTransactions called with {Count} transactions", result.Count);

			List<TransactionVM> model = result.Select(mapper.Map<Transaction, TransactionVM>).ToList();
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Get transactions by receipt ID
	/// </summary>
	/// <param name="receiptId">The ID of the receipt</param>
	/// <returns>A list of transactions associated with the receipt</returns>
	/// <response code="200">Returns the list of transactions</response>
	/// <response code="404">If no transactions are found for the receipt</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetByReceiptId)]
	[ProducesResponseType(typeof(List<TransactionVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionVM>?>> GetTransactionsByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetTransactionsByReceiptId called with receiptId: {ReceiptId}", receiptId);
			GetTransactionsByReceiptIdQuery query = new(receiptId);
			List<Transaction>? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetTransactionsByReceiptId called with receiptId: {ReceiptId} not found", receiptId);
				return NotFound();
			}

			List<TransactionVM> model = result.Select(mapper.Map<Transaction, TransactionVM>).ToList();
			logger.LogDebug("GetTransactionsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTransactionsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Create new transactions
	/// </summary>
	/// <param name="models">The transactions to create</param>
	/// <param name="receiptId">The ID of the associated receipt</param>
	/// <param name="accountId">The ID of the associated account</param>
	/// <returns>The created transactions</returns>
	/// <response code="200">Returns the created transactions</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPost(RouteCreate)]
	[ProducesResponseType(typeof(List<TransactionVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionVM>>> CreateTransactions([FromBody] List<TransactionVM> models, [FromRoute] Guid receiptId, [FromRoute] Guid accountId)
	{
		try
		{
			logger.LogDebug("CreateTransaction called with {Count} transactions", models.Count);
			CreateTransactionCommand command = new(models.Select(mapper.Map<TransactionVM, Transaction>).ToList(), receiptId, accountId);
			List<Transaction> transactions = await mediator.Send(command);
			logger.LogDebug("CreateTransaction called with {Count} transactions, and created", models.Count);
			return Ok(transactions.Select(mapper.Map<Transaction, TransactionVM>).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Update existing transactions
	/// </summary>
	/// <param name="models">The transactions to update</param>
	/// <param name="receiptId">The ID of the associated receipt</param>
	/// <param name="accountId">The ID of the associated account</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the transactions were successfully updated</response>
	/// <response code="404">If the transactions were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPut(RouteUpdate)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateTransactions([FromBody] List<TransactionVM> models, [FromRoute] Guid receiptId, [FromRoute] Guid accountId)
	{
		try
		{
			logger.LogDebug("UpdateTransactions called with {Count} transactions", models.Count);
			UpdateTransactionCommand command = new(models.Select(mapper.Map<TransactionVM, Transaction>).ToList(), receiptId, accountId);
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
			logger.LogError(ex, MessageWithoutId, nameof(UpdateTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Delete transactions
	/// </summary>
	/// <param name="ids">The IDs of the transactions to delete</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the transactions were successfully deleted</response>
	/// <response code="404">If the transactions were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpDelete(RouteDelete)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
			logger.LogError(ex, MessageWithoutId, nameof(DeleteTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}