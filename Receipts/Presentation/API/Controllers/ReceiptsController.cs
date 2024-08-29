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
public class ReceiptsController(IMediator mediator, IMapper mapper, ILogger<ReceiptsController> logger) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<ReceiptsController> _logger = logger;

	[HttpGet("{id}")]
	public async Task<ActionResult<ReceiptVM>> GetReceiptById(Guid id)
	{
		_logger.LogDebug("GetReceiptById called with id: {Id}", id);
		GetReceiptByIdQuery query = new(id);
		Receipt? result = await _mediator.Send(query);

		if (result == null)
		{
			_logger.LogWarning("GetReceiptById called with id: {Id} not found", id);
			return NotFound();
		}

		ReceiptVM model = _mapper.Map<ReceiptVM>(result);
		_logger.LogDebug("GetReceiptById called with id: {Id} found", id);
		return Ok(model);
	}

	[HttpGet]
	public async Task<ActionResult<List<ReceiptVM>>> GetAllReceipts()
	{
		_logger.LogDebug("GetAllReceipts called");
		GetAllReceiptsQuery query = new();
		List<Receipt> result = await _mediator.Send(query);
		_logger.LogDebug("GetAllReceipts called with {Count} receipts", result.Count);
		return Ok(result);
	}

	[HttpPost]
	public async Task<ActionResult<ReceiptVM>> CreateReceipt(List<ReceiptVM> models)
	{
		_logger.LogDebug("CreateReceipt called with {Count} receipts", models.Count);
		CreateReceiptCommand command = new(models.Select(_mapper.Map<ReceiptVM, Receipt>).ToList());
		List<Receipt> receipts = await _mediator.Send(command);
		_logger.LogDebug("CreateReceipt called with {Count} receipts, and created", models.Count);
		return Ok(receipts.Select(_mapper.Map<Receipt, ReceiptVM>).ToList());
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateReceipts(List<ReceiptVM> models)
	{
		_logger.LogDebug("UpdateReceipts called with {Count} receipts", models.Count);
		UpdateReceiptCommand command = new(models.Select(_mapper.Map<ReceiptVM, Receipt>).ToList());
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("UpdateReceipts called with {Count} receipts, but not found", models.Count);
			return NotFound();
		}

		_logger.LogDebug("UpdateReceipts called with {Count} receipts, and found", models.Count);

		return NoContent();
	}

	[HttpDelete]
	public async Task<ActionResult<bool>> DeleteReceipts([FromBody] List<Guid> ids)
	{
		_logger.LogDebug("DeleteReceipts called with {Count} receipt ids", ids.Count);
		DeleteReceiptCommand command = new(ids);
		bool result = await _mediator.Send(command);

		if (!result)
		{
			_logger.LogWarning("DeleteReceipts called with {Count} receipt ids, but not found", ids.Count);
			return NotFound();
		}

		_logger.LogDebug("DeleteReceipts called with {Count} receipt ids, and found", ids.Count);

		return NoContent();
	}
}