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

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<ReceiptWithItemsVM>> GetReceiptWithItemsByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetReceiptWithItemsByReceiptId called with receiptId: {receiptId}", receiptId);
			GetReceiptWithItemsByReceiptIdQuery query = new(receiptId);
			ReceiptWithItems? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetReceiptWithItemsByReceiptId called with receiptId: {receiptId} not found", receiptId);
				return NotFound();
			}

			ReceiptWithItemsVM model = mapper.Map<ReceiptWithItems, ReceiptWithItemsVM>(result);
			logger.LogDebug("GetReceiptWithItemsByReceiptId called with receiptId: {receiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetReceiptWithItemsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
