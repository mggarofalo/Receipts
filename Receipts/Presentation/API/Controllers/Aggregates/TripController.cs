using Application.Queries.Aggregates.Trips;
using AutoMapper;
using Domain.Aggregates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Aggregates;

namespace API.Controllers.Aggregates;

[ApiController]
[Route("api/[controller]")]
public class TripController(IMediator mediator, IMapper mapper, ILogger<TripController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for receiptId: {receiptId}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	private readonly IMediator _mediator = mediator;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<TripController> _logger = logger;

	[HttpGet("by-receipt-id/{receiptId}")]
	public async Task<ActionResult<TripVM>> GetTripByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			_logger.LogDebug("GetTripById called with receiptId: {receiptId}", receiptId);
			GetTripByReceiptIdQuery query = new(receiptId);
			Trip? result = await _mediator.Send(query);

			if (result == null)
			{
				_logger.LogWarning("GetTripById called with receiptId: {receiptId} not found", receiptId);
				return NotFound();
			}

			TripVM model = _mapper.Map<Trip, TripVM>(result);
			_logger.LogDebug("GetTripById called with receiptId: {receiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, MessageWithId, nameof(GetTripByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
