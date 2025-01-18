using Application.Queries.Aggregates.Trips;
using AutoMapper;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Aggregates;

namespace API.Controllers.Aggregates;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TripController(IMediator mediator, IMapper mapper, ILogger<TripController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for receiptId: {receiptId}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteByReceiptId = "by-receipt-id/{receiptId}";

	/// <summary>
	/// Get a trip by receipt ID
	/// </summary>
	/// <param name="receiptId">The ID of the receipt</param>
	/// <returns>The trip associated with the receipt</returns>
	/// <response code="200">Returns the trip</response>
	/// <response code="404">If the trip is not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteByReceiptId)]
	[ProducesResponseType(typeof(TripVM), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TripVM>> GetTripByReceiptId([FromRoute] Guid receiptId)
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

			TripVM model = mapper.Map<Trip, TripVM>(result);
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
