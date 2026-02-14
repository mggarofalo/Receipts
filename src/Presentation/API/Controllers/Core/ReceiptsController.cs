using API.Mapping.Core;
using Application.Commands.Receipt.Create;
using Application.Commands.Receipt.Delete;
using Application.Commands.Receipt.Update;
using Application.Queries.Core.Receipt;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReceiptsController(IMediator mediator, ReceiptMapper mapper, ILogger<ReceiptsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteUpdate = "";
	public const string RouteDelete = "";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a receipt by ID")]
	[EndpointDescription("Returns a single receipt matching the provided GUID.")]
	[ProducesResponseType<ReceiptVM>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ReceiptVM>> GetReceiptById([FromRoute] Guid id)
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

			ReceiptVM model = mapper.ToViewModel(result);
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
	[ProducesResponseType<List<ReceiptVM>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptVM>>> GetAllReceipts()
	{
		try
		{
			logger.LogDebug("GetAllReceipts called");
			GetAllReceiptsQuery query = new();
			List<Receipt> result = await mediator.Send(query);
			logger.LogDebug("GetAllReceipts called with {Count} receipts", result.Count);

			List<ReceiptVM> model = result.Select(mapper.ToViewModel).ToList();
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create receipts")]
	[EndpointDescription("Creates one or more receipts from the provided list and returns the created receipts with their assigned IDs.")]
	[ProducesResponseType<List<ReceiptVM>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ReceiptVM>>> CreateReceipts([FromBody] List<ReceiptVM> models)
	{
		try
		{
			logger.LogDebug("CreateReceipt called with {Count} receipts", models.Count);
			CreateReceiptCommand command = new(models.Select(mapper.ToDomain).ToList());
			List<Receipt> receipts = await mediator.Send(command);
			logger.LogDebug("CreateReceipt called with {Count} receipts, and created", models.Count);
			return Ok(receipts.Select(mapper.ToViewModel).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update receipts")]
	[EndpointDescription("Updates one or more existing receipts. Returns 404 if any receipt in the list is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateReceipts([FromBody] List<ReceiptVM> models)
	{
		try
		{
			logger.LogDebug("UpdateReceipts called with {Count} receipts", models.Count);
			UpdateReceiptCommand command = new(models.Select(mapper.ToDomain).ToList());
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateReceipts called with {Count} receipts, but not found", models.Count);
				return NotFound();
			}

			logger.LogDebug("UpdateReceipts called with {Count} receipts, and found", models.Count);

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

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
