using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Adjustment.Create;
using Application.Commands.Adjustment.Delete;
using Application.Commands.Adjustment.Restore;
using Application.Commands.Adjustment.Update;
using Application.Models;
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
public class AdjustmentsController(IMediator mediator, AdjustmentMapper mapper, ILogger<AdjustmentsController> logger, IEntityChangeNotifier notifier) : ControllerBase
{
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "~/api/receipts/{receiptId}/adjustments";
	public const string RouteUpdate = "{id}";
	public const string RouteDelete = "";
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get an adjustment by ID")]
	[EndpointDescription("Returns a single adjustment matching the provided GUID.")]
	[ProducesResponseType<AdjustmentResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<AdjustmentResponse>> GetAdjustmentById([FromRoute] Guid id)
	{
		GetAdjustmentByIdQuery query = new(id);
		Adjustment? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Adjustment {Id} not found", id);
			return NotFound();
		}

		AdjustmentResponse model = mapper.ToResponse(result);
		return Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all adjustments")]
	[ProducesResponseType<AdjustmentListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<AdjustmentListResponse>> GetAllAdjustments([FromQuery] Guid? receiptId = null, [FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		if (receiptId.HasValue)
		{
			GetAdjustmentsByReceiptIdQuery byReceiptQuery = new(receiptId.Value, offset, limit);
			PagedResult<Adjustment> byReceiptResult = await mediator.Send(byReceiptQuery);

			return Ok(new AdjustmentListResponse
			{
				Data = [.. byReceiptResult.Data.Select(mapper.ToResponse)],
				Total = byReceiptResult.Total,
				Offset = byReceiptResult.Offset,
				Limit = byReceiptResult.Limit,
			});
		}

		GetAllAdjustmentsQuery query = new(offset, limit);
		PagedResult<Adjustment> result = await mediator.Send(query);

		return Ok(new AdjustmentListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted adjustments")]
	[EndpointDescription("Returns all adjustments that have been soft-deleted.")]
	[ProducesResponseType<AdjustmentListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<AdjustmentListResponse>> GetDeletedAdjustments([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedAdjustmentsQuery query = new(offset, limit);
		PagedResult<Adjustment> result = await mediator.Send(query);

		return Ok(new AdjustmentListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single adjustment")]
	[ProducesResponseType<AdjustmentResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<AdjustmentResponse>> CreateAdjustment([FromBody] CreateAdjustmentRequest model, [FromRoute] Guid receiptId)
	{
		CreateAdjustmentCommand command = new([mapper.ToDomain(model)], receiptId);
		List<Adjustment> adjustments = await mediator.Send(command);
		await notifier.NotifyCreated("adjustment", adjustments[0].Id);
		return Ok(mapper.ToResponse(adjustments[0]));
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single adjustment")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateAdjustment([FromBody] UpdateAdjustmentRequest model, [FromRoute] Guid id)
	{
		UpdateAdjustmentCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Adjustment {Id} not found for update", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("adjustment", id);
		return NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete adjustments")]
	[EndpointDescription("Deletes one or more adjustments by their IDs. Returns 404 if any adjustment is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> DeleteAdjustments([FromBody] List<Guid> ids)
	{
		DeleteAdjustmentCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Adjustments delete failed — not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("adjustment", "deleted", ids);
		return NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted adjustment")]
	[EndpointDescription("Restores a previously soft-deleted adjustment by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RestoreAdjustment([FromRoute] Guid id)
	{
		RestoreAdjustmentCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Adjustment {Id} not found or not deleted for restore", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("adjustment", id);
		return NoContent();
	}
}
