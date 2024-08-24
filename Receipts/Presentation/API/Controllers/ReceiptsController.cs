using Application.Commands.Receipt;
using Application.Queries.Receipt;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController(IMediator mediator) : ControllerBase
{
	private readonly IMediator _mediator = mediator;

	[HttpPost]
	public async Task<ActionResult<Guid>> CreateReceipt(CreateReceiptCommand command)
	{
		Guid result = await _mediator.Send(command);
		return Ok(result);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Receipt>> GetReceiptById(Guid id)
	{
		GetReceiptByIdQuery query = new(id);
		Receipt? result = await _mediator.Send(query);

		if (result == null)
		{
			return NotFound();
		}

		return Ok(result);
	}
}
