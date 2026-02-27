using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Subcategory.Create;
using Application.Commands.Subcategory.Delete;
using Application.Commands.Subcategory.Restore;
using Application.Commands.Subcategory.Update;
using Application.Queries.Core.Subcategory;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Core;

[ApiController]
[Route("api/subcategories")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class SubcategoriesController(IMediator mediator, SubcategoryMapper mapper, ILogger<SubcategoriesController> logger) : ControllerBase
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
	public const string RouteGetDeleted = "deleted";
	public const string RouteRestore = "{id}/restore";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a subcategory by ID")]
	[EndpointDescription("Returns a single subcategory matching the provided GUID.")]
	[ProducesResponseType<SubcategoryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<SubcategoryResponse>> GetSubcategoryById([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("GetSubcategoryById called with id: {Id}", id);
			GetSubcategoryByIdQuery query = new(id);
			Subcategory? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetSubcategoryById called with id: {Id} not found", id);
				return NotFound();
			}

			SubcategoryResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetSubcategoryById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetSubcategoryById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all subcategories")]
	[ProducesResponseType<List<SubcategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<SubcategoryResponse>>> GetAllSubcategories()
	{
		try
		{
			logger.LogDebug("GetAllSubcategories called");
			GetAllSubcategoriesQuery query = new();
			List<Subcategory> result = await mediator.Send(query);
			logger.LogDebug("GetAllSubcategories called with {Count} subcategories", result.Count);

			List<SubcategoryResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllSubcategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted subcategories")]
	[EndpointDescription("Returns all subcategories that have been soft-deleted.")]
	[ProducesResponseType<List<SubcategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<SubcategoryResponse>>> GetDeletedSubcategories()
	{
		try
		{
			logger.LogDebug("GetDeletedSubcategories called");
			GetDeletedSubcategoriesQuery query = new();
			List<Subcategory> result = await mediator.Send(query);
			logger.LogDebug("GetDeletedSubcategories called with {Count} subcategories", result.Count);

			List<SubcategoryResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetDeletedSubcategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("~/api/categories/{categoryId}/subcategories")]
	[EndpointSummary("Get subcategories by category ID")]
	[EndpointDescription("Returns all subcategories belonging to the specified category.")]
	[ProducesResponseType<List<SubcategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<SubcategoryResponse>>> GetSubcategoriesByCategoryId([FromRoute] Guid categoryId)
	{
		try
		{
			logger.LogDebug("GetSubcategoriesByCategoryId called with categoryId: {CategoryId}", categoryId);
			GetSubcategoriesByCategoryIdQuery query = new(categoryId);
			List<Subcategory> result = await mediator.Send(query);
			logger.LogDebug("GetSubcategoriesByCategoryId called with {Count} subcategories", result.Count);

			List<SubcategoryResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetSubcategoriesByCategoryId), categoryId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single subcategory")]
	[ProducesResponseType<SubcategoryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<SubcategoryResponse>> CreateSubcategory([FromBody] CreateSubcategoryRequest model)
	{
		try
		{
			logger.LogDebug("CreateSubcategory called");
			CreateSubcategoryCommand command = new([mapper.ToDomain(model)]);
			List<Subcategory> subcategories = await mediator.Send(command);
			return Ok(mapper.ToResponse(subcategories[0]));
		}
		catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
		{
			logger.LogWarning(ex, "Duplicate subcategory name in {Method}", nameof(CreateSubcategory));
			return Conflict("A subcategory with this name already exists in this category.");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateSubcategory));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create subcategories in batch")]
	[ProducesResponseType<List<SubcategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<SubcategoryResponse>>> CreateSubcategories([FromBody] List<CreateSubcategoryRequest> models)
	{
		try
		{
			logger.LogDebug("CreateSubcategories called with {Count} subcategories", models.Count);
			CreateSubcategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
			List<Subcategory> subcategories = await mediator.Send(command);
			return Ok(subcategories.Select(mapper.ToResponse).ToList());
		}
		catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
		{
			logger.LogWarning(ex, "Duplicate subcategory name in {Method}", nameof(CreateSubcategories));
			return Conflict("A subcategory with this name already exists in this category.");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateSubcategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single subcategory")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateSubcategory([FromRoute] Guid id, [FromBody] UpdateSubcategoryRequest model)
	{
		try
		{
			logger.LogDebug("UpdateSubcategory called for id: {Id}", id);
			UpdateSubcategoryCommand command = new([mapper.ToDomain(model)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateSubcategory called for id: {Id}, but not found", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(UpdateSubcategory), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update subcategories in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateSubcategories([FromBody] List<UpdateSubcategoryRequest> models)
	{
		try
		{
			logger.LogDebug("UpdateSubcategories called with {Count} subcategories", models.Count);
			UpdateSubcategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateSubcategories called with {Count} subcategories, but not found", models.Count);
				return NotFound();
			}

			logger.LogDebug("UpdateSubcategories called with {Count} subcategories, and found", models.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateSubcategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete subcategories")]
	[EndpointDescription("Deletes one or more subcategories by their IDs. Returns 404 if any subcategory is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> DeleteSubcategories([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteSubcategories called with {Count} ids", ids.Count);
			DeleteSubcategoryCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteSubcategories called with {Count} ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteSubcategories called with {Count} ids, and found", ids.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteSubcategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted subcategory")]
	[EndpointDescription("Restores a previously soft-deleted subcategory by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> RestoreSubcategory([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("RestoreSubcategory called with id: {Id}", id);
			RestoreSubcategoryCommand command = new(id);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("RestoreSubcategory called with id: {Id}, but not found or not deleted", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(RestoreSubcategory), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
