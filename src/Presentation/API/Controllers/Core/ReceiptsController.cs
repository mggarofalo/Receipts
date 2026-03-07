using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Receipt.Create;
using Application.Commands.Receipt.Delete;
using Application.Commands.Receipt.Restore;
using Application.Commands.Receipt.Update;
using Application.Models;
using Application.Queries.Core.Receipt;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/receipts")]
[Produces("application/json")]
[Authorize]
public class ReceiptsController(
	IMediator mediator,
	ReceiptMapper mapper,
	ILogger<ReceiptsController> logger,
	IEntityChangeNotifier notifier) : ControllerBase
{
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteCreateBatch = "batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "";
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a receipt by ID")]
	[EndpointDescription("Returns a single receipt matching the provided GUID.")]
	public async Task<Results<Ok<ReceiptResponse>, NotFound>> GetReceiptById([FromRoute] Guid id)
	{
		GetReceiptByIdQuery query = new(id);
		Receipt? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Receipt {Id} not found", id);
			return TypedResults.NotFound();
		}

		ReceiptResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all receipts")]
	public async Task<Results<Ok<ReceiptListResponse>, BadRequest<string>>> GetAllReceipts([FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
	{
		if (sortBy is not null && !SortableColumns.Receipt.Contains(sortBy))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sortBy}'. Allowed: {string.Join(", ", SortableColumns.Receipt)}");
		}

		SortParams sort = new(sortBy, sortDirection);
		GetAllReceiptsQuery query = new(offset, limit, sort);
		PagedResult<Receipt> result = await mediator.Send(query);

		return TypedResults.Ok(new ReceiptListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted receipts")]
	[EndpointDescription("Returns all receipts that have been soft-deleted.")]
	public async Task<Results<Ok<ReceiptListResponse>, BadRequest<string>>> GetDeletedReceipts([FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
	{
		if (sortBy is not null && !SortableColumns.Receipt.Contains(sortBy))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sortBy}'. Allowed: {string.Join(", ", SortableColumns.Receipt)}");
		}

		SortParams sort = new(sortBy, sortDirection);
		GetDeletedReceiptsQuery query = new(offset, limit, sort);
		PagedResult<Receipt> result = await mediator.Send(query);

		return TypedResults.Ok(new ReceiptListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single receipt")]
	public async Task<Ok<ReceiptResponse>> CreateReceipt([FromBody] CreateReceiptRequest model)
	{
		CreateReceiptCommand command = new([mapper.ToDomain(model)]);
		List<Receipt> receipts = await mediator.Send(command);
		ReceiptResponse response = mapper.ToResponse(receipts[0]);
		await notifier.NotifyCreated("receipt", receipts[0].Id);
		return TypedResults.Ok(response);
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create receipts in batch")]
	public async Task<Ok<List<ReceiptResponse>>> CreateReceipts([FromBody] List<CreateReceiptRequest> models)
	{
		CreateReceiptCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Receipt> receipts = await mediator.Send(command);
		List<ReceiptResponse> responses = receipts.Select(mapper.ToResponse).ToList();
		await notifier.NotifyBulkChanged("receipt", "created", receipts.Select(r => r.Id));
		return TypedResults.Ok(responses);
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single receipt")]
	public async Task<Results<NoContent, NotFound>> UpdateReceipt([FromRoute] Guid id, [FromBody] UpdateReceiptRequest model)
	{
		UpdateReceiptCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipt {Id} not found for update", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("receipt", id);
		return TypedResults.NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update receipts in batch")]
	public async Task<Results<NoContent, NotFound>> UpdateReceipts([FromBody] List<UpdateReceiptRequest> models)
	{
		UpdateReceiptCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipts batch update failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("receipt", "updated", models.Select(m => m.Id));
		return TypedResults.NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete receipts")]
	[EndpointDescription("Deletes one or more receipts by their IDs. Returns 404 if any receipt is not found.")]
	public async Task<Results<NoContent, NotFound>> DeleteReceipts([FromBody] List<Guid> ids)
	{
		DeleteReceiptCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipts delete failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("receipt", "deleted", ids);
		return TypedResults.NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted receipt")]
	[EndpointDescription("Restores a previously soft-deleted receipt and its associated items and transactions.")]
	public async Task<Results<NoContent, NotFound>> RestoreReceipt([FromRoute] Guid id)
	{
		RestoreReceiptCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipt {Id} not found or not deleted for restore", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("receipt", id);
		return TypedResults.NoContent();
	}
}
