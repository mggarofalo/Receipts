using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Transaction.Create;
using Application.Commands.Transaction.Delete;
using Application.Commands.Transaction.Restore;
using Application.Commands.Transaction.Update;
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
	[ProducesResponseType<List<TransactionResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TransactionResponse>>> GetAllTransactions([FromQuery] Guid? receiptId = null)
	{
		if (receiptId.HasValue)
		{
			GetTransactionsByReceiptIdQuery byReceiptQuery = new(receiptId.Value);
			List<Transaction>? byReceiptResult = await mediator.Send(byReceiptQuery);

			List<TransactionResponse> byReceiptModel = [.. (byReceiptResult ?? []).Select(mapper.ToResponse)];
			return Ok(byReceiptModel);
		}

		GetAllTransactionsQuery query = new();
		List<Transaction> result = await mediator.Send(query);

		List<TransactionResponse> model = [.. result.Select(mapper.ToResponse)];
		return Ok(model);
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted transactions")]
	[EndpointDescription("Returns all transactions that have been soft-deleted.")]
	[ProducesResponseType<List<TransactionResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TransactionResponse>>> GetDeletedTransactions()
	{
		GetDeletedTransactionsQuery query = new();
		List<Transaction> result = await mediator.Send(query);

		List<TransactionResponse> model = [.. result.Select(mapper.ToResponse)];
		return Ok(model);
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single transaction")]
	[ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<TransactionResponse>> CreateTransaction([FromBody] CreateTransactionRequest model, [FromRoute] Guid receiptId)
	{
		CreateTransactionCommand command = new([mapper.ToDomain(model)], receiptId, model.AccountId);
		List<Transaction> transactions = await mediator.Send(command);
		return Ok(mapper.ToResponse(transactions[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create transactions in batch")]
	[ProducesResponseType<List<TransactionResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TransactionResponse>>> CreateTransactions([FromBody] List<CreateTransactionRequest> models, [FromRoute] Guid receiptId)
	{
		IEnumerable<IGrouping<Guid, CreateTransactionRequest>> groups = models.GroupBy(m => m.AccountId);

		List<Task<List<Transaction>>> tasks = [.. groups.Select(group =>
		{
			CreateTransactionCommand command = new(
				[.. group.Select(mapper.ToDomain)],
				receiptId,
				group.Key);
			return mediator.Send(command);
		})];

		await Task.WhenAll(tasks);

		List<TransactionResponse> results = [.. tasks.SelectMany(t => t.Result.Select(mapper.ToResponse))];
		return Ok(results);
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single transaction")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateTransaction([FromBody] UpdateTransactionRequest model, [FromRoute] Guid id)
	{
		UpdateTransactionCommand command = new([mapper.ToDomain(model)], model.AccountId);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Transaction {Id} not found for update", id);
			return NotFound();
		}

		return NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update transactions in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateTransactions([FromBody] List<UpdateTransactionRequest> models)
	{
		IEnumerable<IGrouping<Guid, UpdateTransactionRequest>> groups = models.GroupBy(m => m.AccountId);

		List<Task<bool>> tasks = [.. groups.Select(group =>
		{
			UpdateTransactionCommand command = new(
				[.. group.Select(mapper.ToDomain)],
				group.Key);
			return mediator.Send(command);
		})];

		await Task.WhenAll(tasks);

		if (tasks.Any(t => !t.Result))
		{
			logger.LogWarning("Transactions batch update failed — some not found");
			return NotFound();
		}

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

		return NoContent();
	}
}
