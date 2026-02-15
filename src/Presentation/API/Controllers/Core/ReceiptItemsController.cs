using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.ReceiptItem.Create;
using Application.Commands.ReceiptItem.Delete;
using Application.Commands.ReceiptItem.Update;
using Application.Queries.Core.ReceiptItem;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiController]
[Route("api/receipt-items")]
[Produces("application/json")]
public class ReceiptItemsController(IMediator mediator, ReceiptItemMapper mapper, ILogger<ReceiptItemsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteGetByReceiptId = "by-receipt-id/{receiptId}";
	public const string RouteCreate = "{receiptId}";
	public const string RouteCreateBatch = "{receiptId}/batch";
	public const string RouteUpdate = "{receiptId}";
	public const string RouteUpdateBatch = "{receiptId}/batch";
	public const string RouteDelete = "";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a receipt item by ID")]
	[EndpointDescription("Returns a single receipt item matching the provided GUID.")]
	[ProducesResponseType<ReceiptItemResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ReceiptItemResponse>> GetReceiptItemById([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("GetReceiptItemById called with id: {Id}", id);
			GetReceiptItemByIdQuery query = new(id);
			ReceiptItem? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetReceiptItemById called with id: {Id} not found", id);
				return NotFound();
			}

			ReceiptItemResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetReceiptItemById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetReceiptItemById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all receipt items")]
	[ProducesResponseType<List<ReceiptItemResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptItemResponse>>> GetAllReceiptItems()
	{
		try
		{
			logger.LogDebug("GetAllReceiptItems called");
			GetAllReceiptItemsQuery query = new();
			List<ReceiptItem> result = await mediator.Send(query);
			logger.LogDebug("GetAllReceiptItems called with {Count} receipt items", result.Count);

			List<ReceiptItemResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllReceiptItems));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetByReceiptId)]
	[EndpointSummary("Get receipt items by receipt ID")]
	[EndpointDescription("Returns all receipt items associated with the specified receipt.")]
	[ProducesResponseType<List<ReceiptItemResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptItemResponse>?>> GetReceiptItemsByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId}", receiptId);
			GetReceiptItemsByReceiptIdQuery query = new(receiptId);
			List<ReceiptItem>? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId} not found", receiptId);
				return NotFound();
			}

			List<ReceiptItemResponse> model = [.. result.Select(mapper.ToResponse)];
			logger.LogDebug("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetReceiptItemsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single receipt item")]
	[ProducesResponseType<ReceiptItemResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ReceiptItemResponse>> CreateReceiptItem([FromBody] CreateReceiptItemRequest model, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("CreateReceiptItem called");
			CreateReceiptItemCommand command = new([mapper.ToDomain(model)], receiptId);
			List<ReceiptItem> receiptItems = await mediator.Send(command);
			return Ok(mapper.ToResponse(receiptItems[0]));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateReceiptItem));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create receipt items in batch")]
	[ProducesResponseType<List<ReceiptItemResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptItemResponse>>> CreateReceiptItems([FromBody] List<CreateReceiptItemRequest> models, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("CreateReceiptItems called with {Count} receipt items", models.Count);
			CreateReceiptItemCommand command = new([.. models.Select(mapper.ToDomain)], receiptId);
			List<ReceiptItem> receiptItems = await mediator.Send(command);
			return Ok(receiptItems.Select(mapper.ToResponse).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateReceiptItems));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single receipt item")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateReceiptItem([FromBody] UpdateReceiptItemRequest model, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("UpdateReceiptItem called");
			UpdateReceiptItemCommand command = new([mapper.ToDomain(model)], receiptId);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateReceiptItem called, but not found");
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateReceiptItem));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update receipt items in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateReceiptItems([FromBody] List<UpdateReceiptItemRequest> models, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("UpdateReceiptItems called with {Count} receipt items", models.Count);
			UpdateReceiptItemCommand command = new([.. models.Select(mapper.ToDomain)], receiptId);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateReceiptItems called with {Count} receipt items, but not found", models.Count);
				return NotFound();
			}

			logger.LogDebug("UpdateReceiptItems called with {Count} receipt items, and updated", models.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateReceiptItems));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete receipt items")]
	[EndpointDescription("Deletes one or more receipt items by their IDs. Returns 404 if any item is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> DeleteReceiptItems([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteReceiptItems called with {Count} receipt item ids", ids.Count);
			DeleteReceiptItemCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteReceiptItems called with {Count} receipt item ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteReceiptItems called with {Count} receipt item ids, and deleted", ids.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteReceiptItems));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
