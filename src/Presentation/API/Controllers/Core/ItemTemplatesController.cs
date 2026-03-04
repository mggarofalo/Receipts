using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.ItemTemplate.Create;
using Application.Commands.ItemTemplate.Delete;
using Application.Commands.ItemTemplate.Restore;
using Application.Commands.ItemTemplate.Update;
using Application.Models;
using Application.Queries.Core.ItemTemplate;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/item-templates")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ItemTemplatesController(IMediator mediator, ItemTemplateMapper mapper, ILogger<ItemTemplatesController> logger) : ControllerBase
{
	public const string MessageWithId = "Error occurred in {Method} for id: {Id}";
	public const string MessageWithoutId = "Error occurred in {Method}";
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteUpdate = "{id}";
	public const string RouteDelete = "";
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get an item template by ID")]
	[EndpointDescription("Returns a single item template matching the provided GUID.")]
	[ProducesResponseType<ItemTemplateResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ItemTemplateResponse>> GetItemTemplateById([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("GetItemTemplateById called with id: {Id}", id);
			GetItemTemplateByIdQuery query = new(id);
			ItemTemplate? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetItemTemplateById called with id: {Id} not found", id);
				return NotFound();
			}

			ItemTemplateResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetItemTemplateById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetItemTemplateById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all item templates")]
	[ProducesResponseType<ItemTemplateListResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ItemTemplateListResponse>> GetAllItemTemplates([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		try
		{
			logger.LogDebug("GetAllItemTemplates called");
			GetAllItemTemplatesQuery query = new(offset, limit);
			PagedResult<ItemTemplate> result = await mediator.Send(query);
			logger.LogDebug("GetAllItemTemplates called with {Count} item templates", result.Data.Count);

			return Ok(new ItemTemplateListResponse
			{
				Data = [.. result.Data.Select(mapper.ToResponse)],
				Total = result.Total,
				Offset = result.Offset,
				Limit = result.Limit,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllItemTemplates));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted item templates")]
	[EndpointDescription("Returns all item templates that have been soft-deleted.")]
	[ProducesResponseType<ItemTemplateListResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ItemTemplateListResponse>> GetDeletedItemTemplates([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		try
		{
			logger.LogDebug("GetDeletedItemTemplates called");
			GetDeletedItemTemplatesQuery query = new(offset, limit);
			PagedResult<ItemTemplate> result = await mediator.Send(query);
			logger.LogDebug("GetDeletedItemTemplates called with {Count} item templates", result.Data.Count);

			return Ok(new ItemTemplateListResponse
			{
				Data = [.. result.Data.Select(mapper.ToResponse)],
				Total = result.Total,
				Offset = result.Offset,
				Limit = result.Limit,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetDeletedItemTemplates));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single item template")]
	[ProducesResponseType<ItemTemplateResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ItemTemplateResponse>> CreateItemTemplate([FromBody] CreateItemTemplateRequest model)
	{
		try
		{
			logger.LogDebug("CreateItemTemplate called");
			CreateItemTemplateCommand command = new([mapper.ToDomain(model)]);
			List<ItemTemplate> itemTemplates = await mediator.Send(command);
			return Ok(mapper.ToResponse(itemTemplates[0]));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateItemTemplate));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single item template")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateItemTemplate([FromRoute] Guid id, [FromBody] UpdateItemTemplateRequest model)
	{
		try
		{
			logger.LogDebug("UpdateItemTemplate called for id: {Id}", id);
			UpdateItemTemplateCommand command = new([mapper.ToDomain(model)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateItemTemplate called for id: {Id}, but not found", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(UpdateItemTemplate), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete item templates")]
	[EndpointDescription("Deletes one or more item templates by their IDs. Returns 404 if any item template is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> DeleteItemTemplates([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteItemTemplates called with {Count} ids", ids.Count);
			DeleteItemTemplateCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteItemTemplates called with {Count} ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteItemTemplates called with {Count} ids, and found", ids.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteItemTemplates));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted item template")]
	[EndpointDescription("Restores a previously soft-deleted item template by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> RestoreItemTemplate([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("RestoreItemTemplate called with id: {Id}", id);
			RestoreItemTemplateCommand command = new(id);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("RestoreItemTemplate called with id: {Id}, but not found or not deleted", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(RestoreItemTemplate), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
