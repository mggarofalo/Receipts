using Application.Commands.ReceiptItem.Create;
using Application.Commands.ReceiptItem.Update;
using Application.Commands.ReceiptItem.Delete;
using Application.Queries.Core.ReceiptItem;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReceiptItemsController(IMediator mediator, IMapper mapper, ILogger<ReceiptItemsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteGetByReceiptId = "by-receipt-id/{receiptId}";
	public const string RouteCreate = "{receiptId}";
	public const string RouteUpdate = "{receiptId}";
	public const string RouteDelete = "";

	/// <summary>
	/// Get a receipt item by its ID
	/// </summary>
	/// <param name="id">The ID of the receipt item</param>
	/// <returns>The receipt item</returns>
	/// <response code="200">Returns the receipt item</response>
	/// <response code="404">If the receipt item is not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetById)]
	[ProducesResponseType(typeof(ReceiptItemVM), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ReceiptItemVM>> GetReceiptItemById([FromRoute] Guid id)
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

			ReceiptItemVM model = mapper.Map<ReceiptItem, ReceiptItemVM>(result);
			logger.LogDebug("GetReceiptItemById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetReceiptItemById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Get all receipt items
	/// </summary>
	/// <returns>A list of all receipt items</returns>
	/// <response code="200">Returns the list of receipt items</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetAll)]
	[ProducesResponseType(typeof(List<ReceiptItemVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptItemVM>>> GetAllReceiptItems()
	{
		try
		{
			logger.LogDebug("GetAllReceiptItems called");
			GetAllReceiptItemsQuery query = new();
			List<ReceiptItem> result = await mediator.Send(query);
			logger.LogDebug("GetAllReceiptItems called with {Count} receipt items", result.Count);

			List<ReceiptItemVM> model = result.Select(mapper.Map<ReceiptItem, ReceiptItemVM>).ToList();
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllReceiptItems));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Get receipt items by receipt ID
	/// </summary>
	/// <param name="receiptId">The ID of the receipt</param>
	/// <returns>A list of receipt items associated with the receipt</returns>
	/// <response code="200">Returns the list of receipt items</response>
	/// <response code="404">If no receipt items are found for the receipt</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetByReceiptId)]
	[ProducesResponseType(typeof(List<ReceiptItemVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptItemVM>?>> GetReceiptItemsByReceiptId([FromRoute] Guid receiptId)
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

			List<ReceiptItemVM> model = result.Select(mapper.Map<ReceiptItem, ReceiptItemVM>).ToList();
			logger.LogDebug("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetReceiptItemsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Create new receipt items
	/// </summary>
	/// <param name="models">The receipt items to create</param>
	/// <param name="receiptId">The ID of the associated receipt</param>
	/// <returns>The created receipt items</returns>
	/// <response code="200">Returns the created receipt items</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPost(RouteCreate)]
	[ProducesResponseType(typeof(List<ReceiptItemVM>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptItemVM>>> CreateReceiptItems([FromBody] List<ReceiptItemVM> models, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("CreateReceiptItem called with {Count} receipt items", models.Count);
			CreateReceiptItemCommand command = new(models.Select(mapper.Map<ReceiptItem>).ToList(), receiptId);
			List<ReceiptItem> createdReceiptItems = await mediator.Send(command);
			ArgumentNullException.ThrowIfNull(createdReceiptItems);
			logger.LogDebug("CreateReceiptItem called with {Count} receipt items, and created", models.Count);

			List<ReceiptItemVM> createdReceiptItemVMs = createdReceiptItems.Select(mapper.Map<ReceiptItem, ReceiptItemVM>).ToList();
			ArgumentNullException.ThrowIfNull(createdReceiptItemVMs);
			return Ok(createdReceiptItemVMs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateReceiptItems));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	/// <summary>
	/// Update existing receipt items
	/// </summary>
	/// <param name="models">The receipt items to update</param>
	/// <param name="receiptId">The ID of the associated receipt</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the receipt items were successfully updated</response>
	/// <response code="404">If the receipt items were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPut(RouteUpdate)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateReceiptItems([FromBody] List<ReceiptItemVM> models, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("UpdateReceiptItems called with {Count} receipt items", models.Count);
			List<ReceiptItem> receiptItems = models.Select(mapper.Map<ReceiptItem>).ToList();
			UpdateReceiptItemCommand command = new(receiptItems, receiptId);
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

	/// <summary>
	/// Delete receipt items
	/// </summary>
	/// <param name="ids">The IDs of the receipt items to delete</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the receipt items were successfully deleted</response>
	/// <response code="404">If the receipt items were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpDelete(RouteDelete)]
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