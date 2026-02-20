using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Application.Queries.Aggregates.ReceiptsWithItems;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiController]
[Route("api/receipts-with-items")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ReceiptWithItemsController(IMediator mediator, ReceiptWithItemsMapper mapper, ILogger<ReceiptWithItemsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for receiptId: {receiptId}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteByReceiptId = "by-receipt-id/{receiptId}";

	[HttpGet(RouteByReceiptId)]
	[EndpointSummary("Get a receipt with its items")]
	[EndpointDescription("Returns a receipt and all its associated line items as a single aggregate, looked up by receipt ID.")]
	[ProducesResponseType<ReceiptWithItemsResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ReceiptWithItemsResponse>> GetReceiptWithItemsByReceiptId([FromRoute] Guid receiptId)
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

			ReceiptWithItemsResponse model = mapper.ToResponse(result);
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
