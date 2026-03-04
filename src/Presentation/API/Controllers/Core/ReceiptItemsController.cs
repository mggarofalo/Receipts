using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.ReceiptItem.Create;
using Application.Commands.ReceiptItem.Delete;
using Application.Commands.ReceiptItem.Restore;
using Application.Commands.ReceiptItem.Update;
using Application.Models;
using Application.Queries.Core.ReceiptItem;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/receipt-items")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ReceiptItemsController(IMediator mediator, ReceiptItemMapper mapper, ILogger<ReceiptItemsController> logger) : ControllerBase
{
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "~/api/receipts/{receiptId}/receipt-items";
	public const string RouteCreateBatch = "~/api/receipts/{receiptId}/receipt-items/batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "";
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a receipt item by ID")]
	[EndpointDescription("Returns a single receipt item matching the provided GUID.")]
	[ProducesResponseType<ReceiptItemResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ReceiptItemResponse>> GetReceiptItemById([FromRoute] Guid id)
	{
		GetReceiptItemByIdQuery query = new(id);
		ReceiptItem? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("ReceiptItem {Id} not found", id);
			return NotFound();
		}

		ReceiptItemResponse model = mapper.ToResponse(result);
		return Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all receipt items")]
	[ProducesResponseType<ReceiptItemListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ReceiptItemListResponse>> GetAllReceiptItems([FromQuery] Guid? receiptId = null, [FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		if (receiptId.HasValue)
		{
			GetReceiptItemsByReceiptIdQuery byReceiptQuery = new(receiptId.Value, offset, limit);
			PagedResult<ReceiptItem> byReceiptResult = await mediator.Send(byReceiptQuery);

			return Ok(new ReceiptItemListResponse
			{
				Data = [.. byReceiptResult.Data.Select(mapper.ToResponse)],
				Total = byReceiptResult.Total,
				Offset = byReceiptResult.Offset,
				Limit = byReceiptResult.Limit,
			});
		}

		GetAllReceiptItemsQuery query = new(offset, limit);
		PagedResult<ReceiptItem> result = await mediator.Send(query);

		return Ok(new ReceiptItemListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted receipt items")]
	[EndpointDescription("Returns all receipt items that have been soft-deleted.")]
	[ProducesResponseType<ReceiptItemListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ReceiptItemListResponse>> GetDeletedReceiptItems([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedReceiptItemsQuery query = new(offset, limit);
		PagedResult<ReceiptItem> result = await mediator.Send(query);

		return Ok(new ReceiptItemListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single receipt item")]
	[ProducesResponseType<ReceiptItemResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ReceiptItemResponse>> CreateReceiptItem([FromBody] CreateReceiptItemRequest model, [FromRoute] Guid receiptId)
	{
		CreateReceiptItemCommand command = new([mapper.ToDomain(model)], receiptId);
		List<ReceiptItem> receiptItems = await mediator.Send(command);
		return Ok(mapper.ToResponse(receiptItems[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create receipt items in batch")]
	[ProducesResponseType<List<ReceiptItemResponse>>(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<ReceiptItemResponse>>> CreateReceiptItems([FromBody] List<CreateReceiptItemRequest> models, [FromRoute] Guid receiptId)
	{
		CreateReceiptItemCommand command = new([.. models.Select(mapper.ToDomain)], receiptId);
		List<ReceiptItem> receiptItems = await mediator.Send(command);
		return Ok(receiptItems.Select(mapper.ToResponse).ToList());
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single receipt item")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateReceiptItem([FromBody] UpdateReceiptItemRequest model, [FromRoute] Guid id)
	{
		UpdateReceiptItemCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("ReceiptItem {Id} not found for update", id);
			return NotFound();
		}

		return NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update receipt items in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateReceiptItems([FromBody] List<UpdateReceiptItemRequest> models)
	{
		UpdateReceiptItemCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("ReceiptItems batch update failed — not found");
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete receipt items")]
	[EndpointDescription("Deletes one or more receipt items by their IDs. Returns 404 if any item is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> DeleteReceiptItems([FromBody] List<Guid> ids)
	{
		DeleteReceiptItemCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("ReceiptItems delete failed — not found");
			return NotFound();
		}

		return NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted receipt item")]
	[EndpointDescription("Restores a previously soft-deleted receipt item by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RestoreReceiptItem([FromRoute] Guid id)
	{
		RestoreReceiptItemCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("ReceiptItem {Id} not found or not deleted for restore", id);
			return NotFound();
		}

		return NoContent();
	}
}
