using Application.Commands.ReceiptItem;
using Application.Queries.ReceiptItem;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptItemsController(IMediator mediator, IMapper mapper, ILogger<ReceiptItemsController> logger) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<ReceiptItemsController> _logger = logger;

	[HttpGet("{id}")]
	public async Task<ActionResult<ReceiptItemVM>> GetReceiptItemById(Guid id)
	{
		_logger.LogDebug("GetReceiptItemById called with id: {Id}", id);
		GetReceiptItemByIdQuery query = new(id);
		ReceiptItem? result = await _mediator.Send(query);

		if (result == null)
		{
			_logger.LogWarning("GetReceiptItemById called with id: {Id} not found", id);
			return NotFound();
		}

		ReceiptItemVM model = _mapper.Map<ReceiptItemVM>(result);
		_logger.LogDebug("GetReceiptItemById called with id: {Id} found", id);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<ReceiptItemVM>>> GetAllReceiptItems()
	{
		_logger.LogDebug("GetAllReceiptItems called");
		GetAllReceiptItemsQuery query = new();
		List<ReceiptItem> result = await _mediator.Send(query);
		_logger.LogDebug("GetAllReceiptItems called with {Count} receipt items", result.Count);
		return Ok(result);
	}

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<List<ReceiptItemVM>>> GetReceiptItemsByReceiptId(Guid receiptId)
	{
		_logger.LogDebug("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId}", receiptId);
		GetReceiptItemsByReceiptIdQuery query = new(receiptId);
		List<ReceiptItem> result = await _mediator.Send(query);
		_logger.LogDebug("GetReceiptItemsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
		return Ok(result);
	}

	[HttpPost]
	public async Task<ActionResult<ReceiptItemVM>> CreateReceiptItem(List<ReceiptItemVM> models)
	{
		_logger.LogDebug("CreateReceiptItem called with {Count} receipt items", models.Count);
		CreateReceiptItemCommand command = new(models.Select(_mapper.Map<ReceiptItemVM, ReceiptItem>).ToList());
		List<ReceiptItem> receiptitems = await _mediator.Send(command);
		_logger.LogDebug("CreateReceiptItem called with {Count} receipt items, and created", models.Count);
		return Ok(receiptitems.Select(_mapper.Map<ReceiptItem, ReceiptItemVM>).ToList());
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateReceiptItems(List<ReceiptItemVM> models)
	{
		_logger.LogDebug("UpdateReceiptItems called with {Count} receipt items", models.Count);
		UpdateReceiptItemCommand command = new(models.Select(_mapper.Map<ReceiptItemVM, ReceiptItem>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("UpdateReceiptItems called with {Count} receipt items, but not found", models.Count);
			return NotFound();
		}

		_logger.LogDebug("UpdateReceiptItems called with {Count} receipt items, and updated", models.Count);

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteReceiptItems([FromBody] List<Guid> ids)
	{
		_logger.LogDebug("DeleteReceiptItems called with {Count} receipt item ids", ids.Count);
		DeleteReceiptItemCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("DeleteReceiptItems called with {Count} receipt item ids, but not found", ids.Count);
			return NotFound();
		}

		_logger.LogDebug("DeleteReceiptItems called with {Count} receipt item ids, and deleted", ids.Count);

		return NoContent();
	}
}