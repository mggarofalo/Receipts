using Application.Commands.Receipt;
using Application.Queries.Receipt;
using AutoMapper;
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

	[HttpPost]
	public async Task<ActionResult<ReceiptVM>> CreateReceipt(List<ReceiptVM> models)
	{
		CreateReceiptCommand command = new(models.Select(_mapper.Map<ReceiptVM, Receipt>).ToList());
		List<Receipt> receipts = await _mediator.Send(command);
		return Ok(receipts.Select(_mapper.Map<Receipt, ReceiptVM>).ToList());
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateReceipts(List<ReceiptVM> models)
	{
		UpdateReceiptCommand command = new(models.Select(_mapper.Map<ReceiptVM, Receipt>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteReceipts([FromBody] List<Guid> ids)
	{
		DeleteReceiptCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			return NotFound();
		}

		return NoContent();
	}
}