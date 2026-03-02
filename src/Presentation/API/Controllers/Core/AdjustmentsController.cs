using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Adjustment.Create;
using Application.Commands.Adjustment.Delete;
using Application.Commands.Adjustment.Restore;
using Application.Commands.Adjustment.Update;
using Application.Queries.Core.Adjustment;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/adjustments")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AdjustmentsController(IMediator mediator, AdjustmentMapper mapper, ILogger<AdjustmentsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteGetByReceiptId = "by-receipt-id/{receiptId}";
	public const string RouteCreate = "{receiptId}";
	public const string RouteUpdate = "{receiptId}";
	public const string RouteDelete = "";
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get an adjustment by ID")]
	[EndpointDescription("Returns a single adjustment matching the provided GUID.")]
	[ProducesResponseType<AdjustmentResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AdjustmentResponse>> GetAdjustmentById([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("GetAdjustmentById called with id: {Id}", id);
			GetAdjustmentByIdQuery query = new(id);
			Adjustment? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetAdjustmentById called with id: {Id} not found", id);
				return NotFound();
			}

			AdjustmentResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetAdjustmentById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetAdjustmentById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all adjustments")]
	[ProducesResponseType<List<AdjustmentResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<AdjustmentResponse>>> GetAllAdjustments()
	{
		try
		{
			logger.LogDebug("GetAllAdjustments called");
			GetAllAdjustmentsQuery query = new();
			List<Adjustment> result = await mediator.Send(query);
			logger.LogDebug("GetAllAdjustments called with {Count} adjustments", result.Count);

			List<AdjustmentResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllAdjustments));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted adjustments")]
	[EndpointDescription("Returns all adjustments that have been soft-deleted.")]
	[ProducesResponseType<List<AdjustmentResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<AdjustmentResponse>>> GetDeletedAdjustments()
	{
		try
		{
			logger.LogDebug("GetDeletedAdjustments called");
			GetDeletedAdjustmentsQuery query = new();
			List<Adjustment> result = await mediator.Send(query);
			logger.LogDebug("GetDeletedAdjustments called with {Count} adjustments", result.Count);

			List<AdjustmentResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetDeletedAdjustments));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetByReceiptId)]
	[EndpointSummary("Get adjustments by receipt ID")]
	[EndpointDescription("Returns all adjustments associated with the specified receipt.")]
	[ProducesResponseType<List<AdjustmentResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<AdjustmentResponse>?>> GetAdjustmentsByReceiptId([FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("GetAdjustmentsByReceiptId called with receiptId: {ReceiptId}", receiptId);
			GetAdjustmentsByReceiptIdQuery query = new(receiptId);
			List<Adjustment>? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetAdjustmentsByReceiptId called with receiptId: {ReceiptId} not found", receiptId);
				return NotFound();
			}

			List<AdjustmentResponse> model = [.. result.Select(mapper.ToResponse)];
			logger.LogDebug("GetAdjustmentsByReceiptId called with receiptId: {ReceiptId} found", receiptId);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetAdjustmentsByReceiptId), receiptId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single adjustment")]
	[ProducesResponseType<AdjustmentResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AdjustmentResponse>> CreateAdjustment([FromBody] CreateAdjustmentRequest model, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("CreateAdjustment called");
			CreateAdjustmentCommand command = new([mapper.ToDomain(model)], receiptId);
			List<Adjustment> adjustments = await mediator.Send(command);
			return Ok(mapper.ToResponse(adjustments[0]));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateAdjustment));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single adjustment")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateAdjustment([FromBody] UpdateAdjustmentRequest model, [FromRoute] Guid receiptId)
	{
		try
		{
			logger.LogDebug("UpdateAdjustment called");
			UpdateAdjustmentCommand command = new([mapper.ToDomain(model)], receiptId);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateAdjustment called, but not found");
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateAdjustment));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete adjustments")]
	[EndpointDescription("Deletes one or more adjustments by their IDs. Returns 404 if any adjustment is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> DeleteAdjustments([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteAdjustments called with {Count} adjustment ids", ids.Count);
			DeleteAdjustmentCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteAdjustments called with {Count} adjustment ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteAdjustments called with {Count} adjustment ids, and found", ids.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteAdjustments));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted adjustment")]
	[EndpointDescription("Restores a previously soft-deleted adjustment by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> RestoreAdjustment([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("RestoreAdjustment called with id: {Id}", id);
			RestoreAdjustmentCommand command = new(id);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("RestoreAdjustment called with id: {Id}, but not found or not deleted", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(RestoreAdjustment), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
