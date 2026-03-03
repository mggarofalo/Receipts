using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Transaction.Create;
using Application.Commands.Transaction.CreateBatch;
using Application.Commands.Transaction.Delete;
using Application.Commands.Transaction.Restore;
using Application.Commands.Transaction.Update;
using Application.Commands.Transaction.UpdateBatch;
using Application.Queries.Core.Transaction;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class TransactionsController(IMediator mediator, TransactionMapper mapper, ILogger<TransactionsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "~/api/receipts/{receiptId}/transactions";
	public const string RouteCreateBatch = "~/api/receipts/{receiptId}/transactions/batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "";
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a transaction by ID")]
	[EndpointDescription("Returns a single transaction matching the provided GUID.")]
	[ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TransactionResponse>> GetTransactionById([FromRoute] Guid id)
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

			TransactionResponse model = mapper.ToResponse(result);
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
	[ProducesResponseType<List<TransactionResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionResponse>>> GetAllTransactions([FromQuery] Guid? receiptId = null)
	{
		try
		{
			if (receiptId.HasValue)
			{
				logger.LogDebug("GetAllTransactions called with receiptId: {ReceiptId}", receiptId.Value);
				GetTransactionsByReceiptIdQuery byReceiptQuery = new(receiptId.Value);
				List<Transaction>? byReceiptResult = await mediator.Send(byReceiptQuery);

				List<TransactionResponse> byReceiptModel = [.. (byReceiptResult ?? []).Select(mapper.ToResponse)];
				return Ok(byReceiptModel);
			}

			logger.LogDebug("GetAllTransactions called");
			GetAllTransactionsQuery query = new();
			List<Transaction> result = await mediator.Send(query);
			logger.LogDebug("GetAllTransactions called with {Count} transactions", result.Count);

			List<TransactionResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted transactions")]
	[EndpointDescription("Returns all transactions that have been soft-deleted.")]
	[ProducesResponseType<List<TransactionResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionResponse>>> GetDeletedTransactions()
	{
		try
		{
			logger.LogDebug("GetDeletedTransactions called");
			GetDeletedTransactionsQuery query = new();
			List<Transaction> result = await mediator.Send(query);
			logger.LogDebug("GetDeletedTransactions called with {Count} transactions", result.Count);

			List<TransactionResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetDeletedTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single transaction")]
	[ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TransactionResponse>> CreateTransaction([FromBody] CreateTransactionRequest model, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("CreateTransaction called");
			CreateTransactionCommand command = new([mapper.ToDomain(model)], receiptId, model.AccountId);
			List<Transaction> transactions = await mediator.Send(command);
			return Ok(mapper.ToResponse(transactions[0]));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateTransaction));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create transactions in batch")]
	[ProducesResponseType<List<TransactionResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<TransactionResponse>>> CreateTransactions([FromBody] List<CreateTransactionRequest> models, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("CreateTransactions called with {Count} transactions", models.Count);

			List<Transaction> transactions = [.. models.Select(m =>
			{
				Transaction t = mapper.ToDomain(m);
				t.AccountId = m.AccountId;
				return t;
			})];

			CreateTransactionBatchCommand command = new(transactions, receiptId);
			List<Transaction> result = await mediator.Send(command);

			List<TransactionResponse> results = [.. result.Select(mapper.ToResponse)];
			return Ok(results);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateTransactions));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single transaction")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateTransaction([FromBody] UpdateTransactionRequest model, [FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("UpdateTransaction called");
			UpdateTransactionCommand command = new([mapper.ToDomain(model)], model.AccountId);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateTransaction called, but not found");
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateTransaction));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update transactions in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateTransactions([FromBody] List<UpdateTransactionRequest> models)
	{
		try
		{
			logger.LogDebug("UpdateTransactions called with {Count} transactions", models.Count);

			List<Transaction> transactions = [.. models.Select(m =>
			{
				Transaction t = mapper.ToDomain(m);
				t.AccountId = m.AccountId;
				return t;
			})];

			UpdateTransactionBatchCommand command = new(transactions);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateTransactions called with {Count} transactions, but some not found", models.Count);
				return NotFound();
			}

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

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted transaction")]
	[EndpointDescription("Restores a previously soft-deleted transaction by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> RestoreTransaction([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("RestoreTransaction called with id: {Id}", id);
			RestoreTransactionCommand command = new(id);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("RestoreTransaction called with id: {Id}, but not found or not deleted", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(RestoreTransaction), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
