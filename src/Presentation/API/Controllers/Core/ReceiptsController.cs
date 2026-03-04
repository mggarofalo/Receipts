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
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/receipts")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
	[ProducesResponseType<ReceiptResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ReceiptResponse>> GetReceiptById([FromRoute] Guid id)
	{
		GetReceiptByIdQuery query = new(id);
		Receipt? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Receipt {Id} not found", id);
			return NotFound();
		}

		ReceiptResponse model = mapper.ToResponse(result);
		return Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all receipts")]
	[ProducesResponseType<ReceiptListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ReceiptListResponse>> GetAllReceipts([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetAllReceiptsQuery query = new(offset, limit);
		PagedResult<Receipt> result = await mediator.Send(query);

		return Ok(new ReceiptListResponse
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
	[ProducesResponseType<ReceiptListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ReceiptListResponse>> GetDeletedReceipts([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedReceiptsQuery query = new(offset, limit);
		PagedResult<Receipt> result = await mediator.Send(query);

		return Ok(new ReceiptListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single receipt")]
	[ProducesResponseType<ReceiptResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ReceiptResponse>> CreateReceipt([FromBody] CreateReceiptRequest model)
	{
		CreateReceiptCommand command = new([mapper.ToDomain(model)]);
		List<Receipt> receipts = await mediator.Send(command);
		ReceiptResponse response = mapper.ToResponse(receipts[0]);
		await notifier.NotifyCreated("receipt", receipts[0].Id);
		return Ok(response);
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create receipts in batch")]
	[ProducesResponseType<List<ReceiptResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<ReceiptResponse>>> CreateReceipts([FromBody] List<CreateReceiptRequest> models)
	{
		CreateReceiptCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Receipt> receipts = await mediator.Send(command);
		List<ReceiptResponse> responses = receipts.Select(mapper.ToResponse).ToList();
		await notifier.NotifyBulkChanged("receipt", "created", receipts.Select(r => r.Id));
		return Ok(responses);
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single receipt")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateReceipt([FromRoute] Guid id, [FromBody] UpdateReceiptRequest model)
	{
		UpdateReceiptCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipt {Id} not found for update", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("receipt", id);
		return NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update receipts in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateReceipts([FromBody] List<UpdateReceiptRequest> models)
	{
		UpdateReceiptCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipts batch update failed — not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("receipt", "updated", models.Select(m => m.Id));
		return NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete receipts")]
	[EndpointDescription("Deletes one or more receipts by their IDs. Returns 404 if any receipt is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> DeleteReceipts([FromBody] List<Guid> ids)
	{
		DeleteReceiptCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipts delete failed — not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("receipt", "deleted", ids);
		return NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted receipt")]
	[EndpointDescription("Restores a previously soft-deleted receipt and its associated items and transactions.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RestoreReceipt([FromRoute] Guid id)
	{
		RestoreReceiptCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Receipt {Id} not found or not deleted for restore", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("receipt", id);
		return NoContent();
	}
}
