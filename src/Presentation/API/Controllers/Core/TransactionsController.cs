using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Transaction.Create;
using Application.Commands.Transaction.Delete;
using Application.Commands.Transaction.Restore;
using Application.Commands.Transaction.Update;
using Application.Models;
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
public class TransactionsController(IMediator mediator, TransactionMapper mapper, ILogger<TransactionsController> logger, IEntityChangeNotifier notifier) : ControllerBase
{
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
	public async Task<ActionResult<TransactionResponse>> GetTransactionById([FromRoute] Guid id)
	{
		GetTransactionByIdQuery query = new(id);
		Transaction? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Transaction {Id} not found", id);
			return NotFound();
		}

		TransactionResponse model = mapper.ToResponse(result);
		return Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all transactions")]
	[ProducesResponseType<TransactionListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<TransactionListResponse>> GetAllTransactions([FromQuery] Guid? receiptId = null, [FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		if (receiptId.HasValue)
		{
			GetTransactionsByReceiptIdQuery byReceiptQuery = new(receiptId.Value, offset, limit);
			PagedResult<Transaction> byReceiptResult = await mediator.Send(byReceiptQuery);

			return Ok(new TransactionListResponse
			{
				Data = [.. byReceiptResult.Data.Select(mapper.ToResponse)],
				Total = byReceiptResult.Total,
				Offset = byReceiptResult.Offset,
				Limit = byReceiptResult.Limit,
			});
		}

		GetAllTransactionsQuery query = new(offset, limit);
		PagedResult<Transaction> result = await mediator.Send(query);

		return Ok(new TransactionListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted transactions")]
	[EndpointDescription("Returns all transactions that have been soft-deleted.")]
	[ProducesResponseType<TransactionListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<TransactionListResponse>> GetDeletedTransactions([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedTransactionsQuery query = new(offset, limit);
		PagedResult<Transaction> result = await mediator.Send(query);

		return Ok(new TransactionListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single transaction")]
	[ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<TransactionResponse>> CreateTransaction([FromBody] CreateTransactionRequest model, [FromRoute] Guid receiptId)
	{
		Transaction transaction = mapper.ToDomain(model);
		transaction.AccountId = model.AccountId;
		CreateTransactionCommand command = new([transaction], receiptId);
		List<Transaction> transactions = await mediator.Send(command);
		await notifier.NotifyCreated("transaction", transactions[0].Id);
		return Ok(mapper.ToResponse(transactions[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create transactions in batch")]
	[ProducesResponseType<List<TransactionResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TransactionResponse>>> CreateTransactions([FromBody] List<CreateTransactionRequest> models, [FromRoute] Guid receiptId)
	{
		List<Transaction> transactions = [.. models.Select(m =>
		{
			Transaction t = mapper.ToDomain(m);
			t.AccountId = m.AccountId;
			return t;
		})];

		CreateTransactionCommand command = new(transactions, receiptId);
		List<Transaction> result = await mediator.Send(command);

		List<TransactionResponse> results = [.. result.Select(mapper.ToResponse)];
		await notifier.NotifyBulkChanged("transaction", "created", result.Select(t => t.Id));
		return Ok(results);
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single transaction")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateTransaction([FromBody] UpdateTransactionRequest model, [FromRoute] Guid id)
	{
		Transaction transaction = mapper.ToDomain(model);
		transaction.AccountId = model.AccountId;
		UpdateTransactionCommand command = new([transaction]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transaction {Id} not found for update", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("transaction", id);
		return NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update transactions in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateTransactions([FromBody] List<UpdateTransactionRequest> models)
	{
		List<Transaction> transactions = [.. models.Select(m =>
		{
			Transaction t = mapper.ToDomain(m);
			t.AccountId = m.AccountId;
			return t;
		})];

		UpdateTransactionCommand command = new(transactions);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transactions batch update failed — some not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("transaction", "updated", models.Select(m => m.Id));
		return NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete transactions")]
	[EndpointDescription("Deletes one or more transactions by their IDs. Returns 404 if any transaction is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> DeleteTransactions([FromBody] List<Guid> ids)
	{
		DeleteTransactionCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transactions delete failed — not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("transaction", "deleted", ids);
		return NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted transaction")]
	[EndpointDescription("Restores a previously soft-deleted transaction by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RestoreTransaction([FromRoute] Guid id)
	{
		RestoreTransactionCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transaction {Id} not found or not deleted for restore", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("transaction", id);
		return NoContent();
	}
}
