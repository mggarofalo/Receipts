using Application.Commands.Receipt;
using Application.Queries.Core.Receipt;
using AutoMapper;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ViewModels.Core;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController(IMediator mediator, IMapper mapper, ILogger<ReceiptsController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";

	[HttpGet("{id}")]
	public async Task<ActionResult<ReceiptVM>> GetReceiptById(Guid id)
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

			ReceiptVM model = mapper.Map<Receipt, ReceiptVM>(result);
			logger.LogDebug("GetReceiptById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetReceiptById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet]
	public async Task<ActionResult<List<ReceiptVM>>> GetAllReceipts()
	{
		try
		{
			logger.LogDebug("GetAllReceipts called");
			GetAllReceiptsQuery query = new();
			List<Receipt> result = await mediator.Send(query);
			logger.LogDebug("GetAllReceipts called with {Count} receipts", result.Count);

			List<ReceiptVM> model = result.Select(mapper.Map<Receipt, ReceiptVM>).ToList();
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllReceipts));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost]
	public async Task<ActionResult<ReceiptVM>> CreateReceipt(List<ReceiptVM> models)
	{
		try
		{
			logger.LogDebug("CreateReceipt called with {Count} receipts", models.Count);
			CreateReceiptCommand command = new(models.Select(mapper.Map<ReceiptVM, Receipt>).ToList());
			List<Receipt> receipts = await mediator.Send(command);
			logger.LogDebug("CreateReceipt called with {Count} receipts, and created", models.Count);
			return Ok(receipts.Select(mapper.Map<Receipt, ReceiptVM>).ToList());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateReceipt));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut]
	public async Task<ActionResult<bool>> UpdateReceipts(List<ReceiptVM> models)
	{
		try
		{
			logger.LogDebug("UpdateReceipts called with {Count} receipts", models.Count);
			UpdateReceiptCommand command = new(models.Select(mapper.Map<ReceiptVM, Receipt>).ToList());
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

	[HttpDelete]
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