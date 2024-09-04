using Application.Queries.Aggregates.ReceiptsWithItems;
using AutoMapper;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Aggregates;

namespace API.Controllers.Aggregates;

[ApiController]
[Route("api/[controller]")]
public class ReceiptWithItemsController(IMediator mediator, IMapper mapper, ILogger<ReceiptWithItemsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for receiptId: {receiptId}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<ReceiptWithItemsController> _logger = logger;

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<ReceiptWithItemsVM>> GetReceiptWithItemsByReceiptId(Guid receiptId)
	{
		try
		{
			_logger.LogDebug("GetReceiptWithItemsByReceiptId called with receiptId: {receiptId}", receiptId);
			GetReceiptWithItemsByReceiptIdQuery query = new(receiptId);
			ReceiptWithItems? result = await _mediator.Send(query);

			if (result == null)
			{
				_logger.LogWarning("GetReceiptWithItemsByReceiptId called with receiptId: {receiptId} not found", receiptId);
				return NotFound();
			}

			ReceiptWithItemsVM model = _mapper.Map<ReceiptWithItems, ReceiptWithItemsVM>(result);
			_logger.LogDebug("GetReceiptWithItemsByReceiptId called with receiptId: {receiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, MessageWithId, nameof(GetReceiptWithItemsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
