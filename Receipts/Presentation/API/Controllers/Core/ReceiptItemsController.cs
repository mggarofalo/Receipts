using Application.Commands.ReceiptItem;
using Application.Queries.Core.ReceiptItem;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Mapping;
using Shared.ViewModels.Core;

namespace API.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
public class ReceiptItemsController(IMediator mediator, IMapper mapper, ILogger<ReceiptItemsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	[HttpGet("{id}")]
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

	[HttpGet]
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

	[HttpGet("by-receipt-id/{receiptId}")]
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

	[HttpPost("{receiptId}")]
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

	[HttpPut("{receiptId}")]
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

	[HttpDelete]
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