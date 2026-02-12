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

	/// <summary>
	/// Get a receipt by its ID
	/// </summary>
	/// <param name="id">The ID of the receipt</param>
	/// <returns>The receipt</returns>
	/// <response code="200">Returns the receipt</response>
	/// <response code="404">If the receipt is not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetById)]
	[ProducesResponseType(typeof(ReceiptVM), StatusCodes.Status200OK)]
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

	/// <summary>
	/// Get all receipts
	/// </summary>
	/// <returns>A list of all receipts</returns>
	/// <response code="200">Returns the list of receipts</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpGet(RouteGetAll)]
	[ProducesResponseType(typeof(List<ReceiptVM>), StatusCodes.Status200OK)]
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

	/// <summary>
	/// Create new receipts
	/// </summary>
	/// <param name="models">The receipts to create</param>
	/// <returns>The created receipts</returns>
	/// <response code="200">Returns the created receipts</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPost(RouteCreate)]
	[ProducesResponseType(typeof(List<ReceiptVM>), StatusCodes.Status200OK)]
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

	/// <summary>
	/// Update existing receipts
	/// </summary>
	/// <param name="models">The receipts to update</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the receipts were successfully updated</response>
	/// <response code="404">If the receipts were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpPut(RouteUpdate)]
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

	/// <summary>
	/// Delete receipts
	/// </summary>
	/// <param name="ids">The IDs of the receipts to delete</param>
	/// <returns>A status indicating success or failure</returns>
	/// <response code="204">If the receipts were successfully deleted</response>
	/// <response code="404">If the receipts were not found</response>
	/// <response code="500">If there was an internal server error</response>
	[HttpDelete(RouteDelete)]
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
