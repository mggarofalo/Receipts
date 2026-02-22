using API.Generated.Dtos;
using API.Hubs;
using API.Mapping.Core;
using Application.Commands.Receipt.Create;
using Application.Commands.Receipt.Delete;
using Application.Commands.Receipt.Update;
using Application.Queries.Core.Receipt;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace API.Controllers.Core;

[ApiController]
[Route("api/receipts")]
[Produces("application/json")]
public class ReceiptsController(
	IMediator mediator,
	ReceiptMapper mapper,
	ILogger<ReceiptsController> logger,
	IHubContext<ReceiptsHub, IReceiptsHubClient> hubContext) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteCreateBatch = "batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a receipt by ID")]
	[EndpointDescription("Returns a single receipt matching the provided GUID.")]
	[ProducesResponseType<ReceiptResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ReceiptResponse>> GetReceiptById([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("GetReceiptById called with id: {Id}", id);
			GetReceiptByIdQuery query = new(id);
			Receipt? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetReceiptById called with id: {Id} not found", id);
				return NotFound();
			}

			ReceiptResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetReceiptById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetReceiptById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all receipts")]
	[ProducesResponseType<List<ReceiptResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptResponse>>> GetAllReceipts()
	{
		try
		{
			logger.LogDebug("GetAllReceipts called");
			GetAllReceiptsQuery query = new();
			List<Receipt> result = await mediator.Send(query);
			logger.LogDebug("GetAllReceipts called with {Count} receipts", result.Count);

			List<ReceiptResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single receipt")]
	[ProducesResponseType<ReceiptResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ReceiptResponse>> CreateReceipt([FromBody] CreateReceiptRequest model)
	{
		try
		{
			logger.LogDebug("CreateReceipt called");
			CreateReceiptCommand command = new([mapper.ToDomain(model)]);
			List<Receipt> receipts = await mediator.Send(command);
			ReceiptResponse response = mapper.ToResponse(receipts[0]);
			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId != null)
			{
				await hubContext.Clients.Group(userId).ReceiptCreated(response);
			}

			return Ok(response);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateReceipt));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create receipts in batch")]
	[ProducesResponseType<List<ReceiptResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptResponse>>> CreateReceipts([FromBody] List<CreateReceiptRequest> models)
	{
		try
		{
			logger.LogDebug("CreateReceipts called with {Count} receipts", models.Count);
			CreateReceiptCommand command = new([.. models.Select(mapper.ToDomain)]);
			List<Receipt> receipts = await mediator.Send(command);
			List<ReceiptResponse> responses = receipts.Select(mapper.ToResponse).ToList();
			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			foreach (ReceiptResponse response in responses)
			{
				if (userId != null)
				{
					await hubContext.Clients.Group(userId).ReceiptCreated(response);
				}
			}

			return Ok(responses);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single receipt")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateReceipt([FromRoute] Guid id, [FromBody] UpdateReceiptRequest model)
	{
		try
		{
			logger.LogDebug("UpdateReceipt called for id: {Id}", id);
			UpdateReceiptCommand command = new([mapper.ToDomain(model)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateReceipt called for id: {Id}, but not found", id);
				return NotFound();
			}

			GetReceiptByIdQuery fetchQuery = new(id);
			Receipt? updatedReceipt = await mediator.Send(fetchQuery);
			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (updatedReceipt != null && userId != null)
			{
				await hubContext.Clients.Group(userId).ReceiptUpdated(mapper.ToResponse(updatedReceipt));
			}
			else if (updatedReceipt == null)
			{
				logger.LogWarning("UpdateReceipt: receipt {Id} was confirmed updated but could not be fetched for broadcast", id);
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(UpdateReceipt), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	// NOTE: Batch update semantics — UpdateReceiptCommand returns false only when ALL records
	// fail (e.g. none found). A partially-successful batch returns true, so some records may
	// be updated without a corresponding ReceiptUpdated broadcast if their post-update fetch
	// returns null. This is a known observable failure mode via the hub (see MGG-37 review).
	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update receipts in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateReceipts([FromBody] List<UpdateReceiptRequest> models)
	{
		try
		{
			logger.LogDebug("UpdateReceipts called with {Count} receipts", models.Count);
			UpdateReceiptCommand command = new([.. models.Select(mapper.ToDomain)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateReceipts called with {Count} receipts, but not found", models.Count);
				return NotFound();
			}

			logger.LogDebug("UpdateReceipts called with {Count} receipts, and found", models.Count);

			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			foreach (UpdateReceiptRequest model in models)
			{
				GetReceiptByIdQuery fetchQuery = new(model.Id);
				Receipt? updatedReceipt = await mediator.Send(fetchQuery);
				if (updatedReceipt != null && userId != null)
				{
					await hubContext.Clients.Group(userId).ReceiptUpdated(mapper.ToResponse(updatedReceipt));
				}
				else if (updatedReceipt == null)
				{
					logger.LogWarning("UpdateReceipts: receipt {Id} was confirmed updated but could not be fetched for broadcast", model.Id);
				}
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete receipts")]
	[EndpointDescription("Deletes one or more receipts by their IDs. Returns 404 if any receipt is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> DeleteReceipts([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteReceipts called with {Count} receipt ids", ids.Count);
			DeleteReceiptCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteReceipts called with {Count} receipt ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteReceipts called with {Count} receipt ids, and found", ids.Count);

			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			foreach (Guid id in ids)
			{
				if (userId != null)
				{
					await hubContext.Clients.Group(userId).ReceiptDeleted(id);
				}
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
