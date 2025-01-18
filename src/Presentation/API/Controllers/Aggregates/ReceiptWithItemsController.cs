using Application.Queries.Aggregates.ReceiptsWithItems;
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
public class ReceiptWithItemsController(IMediator mediator, IMapper mapper, ILogger<ReceiptWithItemsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for receiptId: {receiptId}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteByReceiptId = "by-receipt-id/{receiptId}";

	/// <summary>
	/// Get a receipt with its items by receipt ID
	/// </summary>
	/// <param name="receiptId">The ID of the receipt</param>
	/// <returns>The receipt with its items</returns>
	/// <response code="200">Returns the receipt with its items</response>
	/// <response code="404">If the receipt is not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteByReceiptId)]
	[ProducesResponseType(typeof(ReceiptWithItemsVM), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
