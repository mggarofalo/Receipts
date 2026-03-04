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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
[Authorize]
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
	public async Task<Results<Ok<TransactionResponse>, NotFound>> GetTransactionById([FromRoute] Guid id)
	{
		GetTransactionByIdQuery query = new(id);
		Transaction? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Transaction {Id} not found", id);
			return TypedResults.NotFound();
		}

		TransactionResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all transactions")]
	public async Task<Ok<TransactionListResponse>> GetAllTransactions([FromQuery] Guid? receiptId = null, [FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		if (receiptId.HasValue)
		{
			GetTransactionsByReceiptIdQuery byReceiptQuery = new(receiptId.Value, offset, limit);
			PagedResult<Transaction> byReceiptResult = await mediator.Send(byReceiptQuery);

			return TypedResults.Ok(new TransactionListResponse
			{
				Data = [.. byReceiptResult.Data.Select(mapper.ToResponse)],
				Total = byReceiptResult.Total,
				Offset = byReceiptResult.Offset,
				Limit = byReceiptResult.Limit,
			});
		}

		GetAllTransactionsQuery query = new(offset, limit);
		PagedResult<Transaction> result = await mediator.Send(query);

		return TypedResults.Ok(new TransactionListResponse
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
	public async Task<Ok<TransactionListResponse>> GetDeletedTransactions([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedTransactionsQuery query = new(offset, limit);
		PagedResult<Transaction> result = await mediator.Send(query);

		return TypedResults.Ok(new TransactionListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single transaction")]
	public async Task<Ok<TransactionResponse>> CreateTransaction([FromBody] CreateTransactionRequest model, [FromRoute] Guid receiptId)
	{
		Transaction transaction = mapper.ToDomain(model);
		transaction.AccountId = model.AccountId;
		CreateTransactionCommand command = new([transaction], receiptId);
		List<Transaction> transactions = await mediator.Send(command);
		await notifier.NotifyCreated("transaction", transactions[0].Id);
		return TypedResults.Ok(mapper.ToResponse(transactions[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create transactions in batch")]
	public async Task<Ok<List<TransactionResponse>>> CreateTransactions([FromBody] List<CreateTransactionRequest> models, [FromRoute] Guid receiptId)
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
		return TypedResults.Ok(results);
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single transaction")]
	public async Task<Results<NoContent, NotFound>> UpdateTransaction([FromBody] UpdateTransactionRequest model, [FromRoute] Guid id)
	{
		Transaction transaction = mapper.ToDomain(model);
		transaction.AccountId = model.AccountId;
		UpdateTransactionCommand command = new([transaction]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transaction {Id} not found for update", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("transaction", id);
		return TypedResults.NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update transactions in batch")]
	public async Task<Results<NoContent, NotFound>> UpdateTransactions([FromBody] List<UpdateTransactionRequest> models)
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
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("transaction", "updated", models.Select(m => m.Id));
		return TypedResults.NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete transactions")]
	[EndpointDescription("Deletes one or more transactions by their IDs. Returns 404 if any transaction is not found.")]
	public async Task<Results<NoContent, NotFound>> DeleteTransactions([FromBody] List<Guid> ids)
	{
		DeleteTransactionCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transactions delete failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("transaction", "deleted", ids);
		return TypedResults.NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted transaction")]
	[EndpointDescription("Restores a previously soft-deleted transaction by clearing its DeletedAt timestamp.")]
	public async Task<Results<NoContent, NotFound>> RestoreTransaction([FromRoute] Guid id)
	{
		RestoreTransactionCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transaction {Id} not found or not deleted for restore", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("transaction", id);
		return TypedResults.NoContent();
	}
}
