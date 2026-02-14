using API.Mapping.Core;
using Application.Commands.Transaction.Create;
using Application.Commands.Transaction.Delete;
using Application.Commands.Transaction.Update;
using Application.Queries.Core.Transaction;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController(IMediator mediator, TransactionMapper mapper, ILogger<TransactionsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteGetByReceiptId = "by-receipt-id/{receiptId}";
	public const string RouteCreate = "{receiptId}/{accountId}";
	public const string RouteUpdate = "{receiptId}/{accountId}";
	public const string RouteDelete = "";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a transaction by ID")]
	[EndpointDescription("Returns a single transaction matching the provided GUID.")]
	[ProducesResponseType<TransactionVM>(StatusCodes.Status200OK)]
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

			TransactionVM model = mapper.ToViewModel(result);
			logger.LogDebug("GetTransactionById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTransactionById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all transactions")]
	[ProducesResponseType<List<TransactionVM>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionVM>>> GetAllTransactions()
	{
		try
		{
			logger.LogDebug("GetAllTransactions called");
			GetAllTransactionsQuery query = new();
			List<Transaction> result = await mediator.Send(query);
			logger.LogDebug("GetAllTransactions called with {Count} transactions", result.Count);

			List<TransactionVM> model = result.Select(mapper.ToViewModel).ToList();
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetByReceiptId)]
	[EndpointSummary("Get transactions by receipt ID")]
	[EndpointDescription("Returns all transactions associated with the specified receipt.")]
	[ProducesResponseType<List<TransactionVM>>(StatusCodes.Status200OK)]
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

			List<TransactionVM> model = result.Select(mapper.ToViewModel).ToList();
			logger.LogDebug("GetTransactionsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTransactionsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create transactions")]
	[EndpointDescription("Creates one or more transactions linked to the specified receipt and account, and returns the created transactions with their assigned IDs.")]
	[ProducesResponseType<List<TransactionVM>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionVM>>> CreateTransactions([FromBody] List<TransactionVM> models, [FromRoute] Guid receiptId, [FromRoute] Guid accountId)
	{
		try
		{
			logger.LogDebug("CreateTransaction called with {Count} transactions", models.Count);
			CreateTransactionCommand command = new(models.Select(mapper.ToDomain).ToList(), receiptId, accountId);
			List<Transaction> transactions = await mediator.Send(command);
			logger.LogDebug("CreateTransaction called with {Count} transactions, and created", models.Count);
			return Ok(transactions.Select(mapper.ToViewModel).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update transactions")]
	[EndpointDescription("Updates one or more transactions linked to the specified receipt and account. Returns 404 if any transaction is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateTransactions([FromBody] List<TransactionVM> models, [FromRoute] Guid receiptId, [FromRoute] Guid accountId)
	{
		try
		{
			logger.LogDebug("UpdateTransactions called with {Count} transactions", models.Count);
			UpdateTransactionCommand command = new(models.Select(mapper.ToDomain).ToList(), receiptId, accountId);
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

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete transactions")]
	[EndpointDescription("Deletes one or more transactions by their IDs. Returns 404 if any transaction is not found.")]
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
