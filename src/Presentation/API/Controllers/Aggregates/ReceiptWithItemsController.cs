using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Application.Queries.Aggregates.ReceiptsWithItems;
using Asp.Versioning;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiVersion("1.0")]
[ApiController]
[Route("api/receipts-with-items")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ReceiptWithItemsController(IMediator mediator, ReceiptWithItemsMapper mapper, ILogger<ReceiptWithItemsController> logger) : ControllerBase
{
	public const string RouteByReceiptId = "";

	[HttpGet(RouteByReceiptId)]
	[EndpointSummary("Get a receipt with its items")]
	[EndpointDescription("Returns a receipt and all its associated line items as a single aggregate, looked up by receipt ID.")]
	[ProducesResponseType<ReceiptWithItemsResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ReceiptWithItemsResponse>> GetReceiptWithItemsByReceiptId([FromQuery] Guid receiptId)
	{
		GetReceiptWithItemsByReceiptIdQuery query = new(receiptId);
		ReceiptWithItems? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("ReceiptWithItems for receipt {ReceiptId} not found", receiptId);
			return NotFound();
		}

		ReceiptWithItemsResponse model = mapper.ToResponse(result);
		return Ok(model);
	}
}
