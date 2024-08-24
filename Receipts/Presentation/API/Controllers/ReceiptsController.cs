using Application.Commands.Receipt;
using Application.Queries.Receipt;
using AutoMapper;
using Domain;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController(IMediator mediator, IMapper mapper) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;

	[HttpPost]
	public async Task<ActionResult<ReceiptVM>> CreateReceipt(ReceiptVM model)
	{
		CreateReceiptCommand command = _mapper.Map<ReceiptVM, CreateReceiptCommand>(model);
		Guid result = await _mediator.Send(command);

		model.Id = result;
		return Ok(model);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<ReceiptVM>> GetReceiptById(Guid id)
	{
		GetReceiptByIdQuery query = new(id);
		Receipt? result = await _mediator.Send(query);

		if (result == null)
		{
			return NotFound();
		}

		ReceiptVM model = _mapper.Map<ReceiptVM>(result);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<ReceiptVM>>> GetAllReceipts()
	{
		GetAllReceiptsQuery query = new();
		List<Receipt> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-location/{location}")]
	public async Task<ActionResult<List<ReceiptVM>>> GetReceiptsByLocation(string location)
	{
		GetReceiptsByLocationQuery query = new(location);
		List<Receipt> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-date")]
	public async Task<ActionResult<List<ReceiptVM>>> GetReceiptsByDateRange(DateTime startDate, DateTime endDate)
	{
		GetReceiptsByDateRangeQuery query = new(startDate, endDate);
		List<Receipt> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("by-money")]
	public async Task<ActionResult<List<ReceiptVM>>> GetReceiptsByMoneyRange(decimal minAmount, decimal maxAmount)
	{
		GetReceiptsByMoneyRangeQuery query = new(new Money(minAmount), new Money(maxAmount));
		List<Receipt> result = await _mediator.Send(query);
		return Ok(result);
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateReceipt(ReceiptVM model)
	{
		UpdateReceiptCommand command = _mapper.Map<ReceiptVM, UpdateReceiptCommand>(model);

		bool result = await _mediator.Send(command);
		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<ActionResult<bool>> DeleteReceipt(Guid id)
	{
		DeleteReceiptCommand command = new(id);
		bool result = await _mediator.Send(command);
		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}
}