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
public class ReceiptItemsController(IMediator mediator, IMapper mapper) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;

	[HttpGet("{id}")]
	public async Task<ActionResult<ReceiptItemVM>> GetReceiptItemById(Guid id)
	{
		GetReceiptItemByIdQuery query = new(id);
		ReceiptItem? result = await _mediator.Send(query);

		if (result == null)
		{
			return NotFound();
		}

		ReceiptItemVM model = _mapper.Map<ReceiptItemVM>(result);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<ReceiptItemVM>>> GetAllReceiptItems()
	{
		GetAllReceiptItemsQuery query = new();
		List<ReceiptItem> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<List<ReceiptItemVM>>> GetReceiptItemsByReceiptId(Guid receiptId)
	{
		GetReceiptItemsByReceiptIdQuery query = new(receiptId);
		List<ReceiptItem> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpPost]
	public async Task<ActionResult<ReceiptItemVM>> CreateReceiptItem(List<ReceiptItemVM> models)
	{
		CreateReceiptItemCommand command = new(models.Select(_mapper.Map<ReceiptItemVM, ReceiptItem>).ToList());
		List<ReceiptItem> receiptitems = await _mediator.Send(command);
		return Ok(receiptitems.Select(_mapper.Map<ReceiptItem, ReceiptItemVM>).ToList());
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateReceiptItems(List<ReceiptItemVM> models)
	{
		UpdateReceiptItemCommand command = new(models.Select(_mapper.Map<ReceiptItemVM, ReceiptItem>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteReceiptItems([FromBody] List<Guid> ids)
	{
		DeleteReceiptItemCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}
}