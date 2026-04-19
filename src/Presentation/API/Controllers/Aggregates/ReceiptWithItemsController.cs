using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Application.Queries.Aggregates.ReceiptsWithItems;
using Asp.Versioning;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiVersion("1.0")]
[ApiController]
[Route("api/receipts-with-items")]
[Produces("application/json")]
[Authorize]
public class ReceiptWithItemsController(IMediator mediator, ReceiptWithItemsMapper mapper, ILogger<ReceiptWithItemsController> logger) : ControllerBase
{
	public const string RouteByReceiptId = "";

	[HttpGet(RouteByReceiptId)]
	[EndpointSummary("Get a receipt with its items")]
	[EndpointDescription("Returns a receipt and all its associated line items as a single aggregate, looked up by receipt ID.")]
	public async Task<Results<Ok<ReceiptWithItemsResponse>, NotFound>> GetReceiptWithItemsByReceiptId([FromQuery] Guid receiptId, CancellationToken cancellationToken = default)
	{
		GetReceiptWithItemsByReceiptIdQuery query = new(receiptId);
		ReceiptWithItems? result = await mediator.Send(query, cancellationToken);

		if (result == null)
		{
			logger.LogWarning("ReceiptWithItems for receipt {ReceiptId} not found", receiptId);
			return TypedResults.NotFound();
		}

		ReceiptWithItemsResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}
}
