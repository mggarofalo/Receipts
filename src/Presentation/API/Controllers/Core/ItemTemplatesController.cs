using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
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
public class ItemTemplatesController(IMediator mediator, ItemTemplateMapper mapper, ILogger<ItemTemplatesController> logger, IEntityChangeNotifier notifier) : ControllerBase
{
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
	public async Task<ActionResult<ItemTemplateResponse>> GetItemTemplateById([FromRoute] Guid id)
	{
		GetItemTemplateByIdQuery query = new(id);
		ItemTemplate? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("ItemTemplate {Id} not found", id);
			return NotFound();
		}

		ItemTemplateResponse model = mapper.ToResponse(result);
		return Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all item templates")]
	[ProducesResponseType<ItemTemplateListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ItemTemplateListResponse>> GetAllItemTemplates([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetAllItemTemplatesQuery query = new(offset, limit);
		PagedResult<ItemTemplate> result = await mediator.Send(query);

		return Ok(new ItemTemplateListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted item templates")]
	[EndpointDescription("Returns all item templates that have been soft-deleted.")]
	[ProducesResponseType<ItemTemplateListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ItemTemplateListResponse>> GetDeletedItemTemplates([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedItemTemplatesQuery query = new(offset, limit);
		PagedResult<ItemTemplate> result = await mediator.Send(query);

		return Ok(new ItemTemplateListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single item template")]
	[ProducesResponseType<ItemTemplateResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<ItemTemplateResponse>> CreateItemTemplate([FromBody] CreateItemTemplateRequest model)
	{
		CreateItemTemplateCommand command = new([mapper.ToDomain(model)]);
		List<ItemTemplate> itemTemplates = await mediator.Send(command);
		await notifier.NotifyCreated("item-template", itemTemplates[0].Id);
		return Ok(mapper.ToResponse(itemTemplates[0]));
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single item template")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateItemTemplate([FromRoute] Guid id, [FromBody] UpdateItemTemplateRequest model)
	{
		UpdateItemTemplateCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("ItemTemplate {Id} not found for update", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("item-template", id);
		return NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete item templates")]
	[EndpointDescription("Deletes one or more item templates by their IDs. Returns 404 if any item template is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> DeleteItemTemplates([FromBody] List<Guid> ids)
	{
		DeleteItemTemplateCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("ItemTemplates delete failed — not found");
			return NotFound();
		}

		await notifier.NotifyBulkChanged("item-template", "deleted", ids);
		return NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted item template")]
	[EndpointDescription("Restores a previously soft-deleted item template by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RestoreItemTemplate([FromRoute] Guid id)
	{
		RestoreItemTemplateCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("ItemTemplate {Id} not found or not deleted for restore", id);
			return NotFound();
		}

		await notifier.NotifyUpdated("item-template", id);
		return NoContent();
	}
}
