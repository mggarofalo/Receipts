using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Application.Queries.Aggregates.Trips;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiController]
[Route("api/trips")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class TripController(IMediator mediator, TripMapper mapper, ILogger<TripController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for receiptId: {receiptId}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteByReceiptId = "by-receipt-id/{receiptId}";

	[HttpGet(RouteByReceiptId)]
	[EndpointSummary("Get a trip by receipt ID")]
	[EndpointDescription("Returns the full trip aggregate for a receipt, including the receipt, its items, transactions, and associated accounts.")]
	[ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TripResponse>> GetTripByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetTripById called with receiptId: {receiptId}", receiptId);
			GetTripByReceiptIdQuery query = new(receiptId);
			Trip? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetTripById called with receiptId: {receiptId} not found", receiptId);
				return NotFound();
			}

			TripResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetTripById called with receiptId: {receiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetTripByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
