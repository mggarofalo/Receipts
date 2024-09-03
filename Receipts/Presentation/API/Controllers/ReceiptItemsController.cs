using Application.Commands.ReceiptItem;
using Application.Queries.Core.ReceiptItem;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptItemsController(IMediator mediator, IMapper mapper, ILogger<ReceiptItemsController> logger) : ControllerBase
{
	[HttpGet("{id}")]
	public async Task<ActionResult<ReceiptItemVM>> GetReceiptItemById(Guid id)
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
			logger.LogError(ex, "Error occurred in GetReceiptItemById for id: {Id}", id);
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
			logger.LogError(ex, "Error occurred in GetAllReceiptItems");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<List<ReceiptItemVM>>> GetReceiptItemsByReceiptId(Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId}", receiptId);
			GetReceiptItemsByReceiptIdQuery query = new(receiptId);
			List<ReceiptItem> result = await mediator.Send(query);
			logger.LogDebug("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
			return Ok(result);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in GetReceiptItemsByReceiptId for receiptId: {ReceiptId}", receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost]
	public async Task<ActionResult<ReceiptItemVM>> CreateReceiptItem(List<ReceiptItemVM> models)
	{
		try
		{
			logger.LogDebug("CreateReceiptItem called with {Count} receipt items", models.Count);
			CreateReceiptItemCommand command = new(models.Select(mapper.Map<ReceiptItemVM, ReceiptItem>).ToList());
			List<ReceiptItem> receiptitems = await mediator.Send(command);
			logger.LogDebug("CreateReceiptItem called with {Count} receipt items, and created", models.Count);
			return Ok(receiptitems.Select(mapper.Map<ReceiptItem, ReceiptItemVM>).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in CreateReceiptItem");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateReceiptItems(List<ReceiptItemVM> models)
	{
		try
		{
			logger.LogDebug("UpdateReceiptItems called with {Count} receipt items", models.Count);
			UpdateReceiptItemCommand command = new(models.Select(mapper.Map<ReceiptItemVM, ReceiptItem>).ToList());
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
			logger.LogError(ex, "Error occurred in UpdateReceiptItems");
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
			logger.LogError(ex, "Error occurred in DeleteReceiptItems");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}