using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Application.Queries.Aggregates.Trips;
using Asp.Versioning;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Aggregates;

[ApiVersion("1.0")]
[ApiController]
[Route("api/trips")]
[Produces("application/json")]
[Authorize]
public class TripController(IMediator mediator, TripMapper mapper, ILogger<TripController> logger) : ControllerBase
{
	public const string RouteByReceiptId = "";

	[HttpGet(RouteByReceiptId)]
	[EndpointSummary("Get a trip by receipt ID")]
	[EndpointDescription("Returns the full trip aggregate for a receipt, including the receipt, its items, transactions, and associated accounts.")]
	public async Task<Results<Ok<TripResponse>, NotFound>> GetTripByReceiptId([FromQuery] Guid receiptId)
	{
		GetTripByReceiptIdQuery query = new(receiptId);
		Trip? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Trip for receipt {ReceiptId} not found", receiptId);
			return TypedResults.NotFound();
		}

		TripResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}
}
