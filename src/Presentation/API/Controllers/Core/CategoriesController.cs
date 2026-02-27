using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Category.Create;
using Application.Commands.Category.Delete;
using Application.Commands.Category.Restore;
using Application.Commands.Category.Update;
using Application.Exceptions;
using Application.Queries.Core.Category;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class CategoriesController(IMediator mediator, CategoryMapper mapper, ILogger<CategoriesController> logger) : ControllerBase
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
	[EndpointSummary("Get a category by ID")]
	[EndpointDescription("Returns a single category matching the provided GUID.")]
	[ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<CategoryResponse>> GetCategoryById([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("GetCategoryById called with id: {Id}", id);
			GetCategoryByIdQuery query = new(id);
			Category? result = await mediator.Send(query);

			if (result == null)
			{
				logger.LogWarning("GetCategoryById called with id: {Id} not found", id);
				return NotFound();
			}

			CategoryResponse model = mapper.ToResponse(result);
			logger.LogDebug("GetCategoryById called with id: {Id} found", id);
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(GetCategoryById), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all categories")]
	[ProducesResponseType<List<CategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<CategoryResponse>>> GetAllCategories()
	{
		try
		{
			logger.LogDebug("GetAllCategories called");
			GetAllCategoriesQuery query = new();
			List<Category> result = await mediator.Send(query);
			logger.LogDebug("GetAllCategories called with {Count} categories", result.Count);

			List<CategoryResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetAllCategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted categories")]
	[EndpointDescription("Returns all categories that have been soft-deleted.")]
	[ProducesResponseType<List<CategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<CategoryResponse>>> GetDeletedCategories()
	{
		try
		{
			logger.LogDebug("GetDeletedCategories called");
			GetDeletedCategoriesQuery query = new();
			List<Category> result = await mediator.Send(query);
			logger.LogDebug("GetDeletedCategories called with {Count} categories", result.Count);

			List<CategoryResponse> model = [.. result.Select(mapper.ToResponse)];
			return Ok(model);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetDeletedCategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single category")]
	[ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest model)
	{
		try
		{
			logger.LogDebug("CreateCategory called");
			CreateCategoryCommand command = new([mapper.ToDomain(model)]);
			List<Category> categories = await mediator.Send(command);
			return Ok(mapper.ToResponse(categories[0]));
		}
		catch (DuplicateEntityException ex)
		{
			logger.LogWarning(ex, "Duplicate category name in {Method}", nameof(CreateCategory));
			return Conflict(ex.Message);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateCategory));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create categories in batch")]
	[ProducesResponseType<List<CategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<CategoryResponse>>> CreateCategories([FromBody] List<CreateCategoryRequest> models)
	{
		try
		{
			logger.LogDebug("CreateCategories called with {Count} categories", models.Count);
			CreateCategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
			List<Category> categories = await mediator.Send(command);
			return Ok(categories.Select(mapper.ToResponse).ToList());
		}
		catch (DuplicateEntityException ex)
		{
			logger.LogWarning(ex, "Duplicate category name in {Method}", nameof(CreateCategories));
			return Conflict(ex.Message);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(CreateCategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single category")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryRequest model)
	{
		try
		{
			logger.LogDebug("UpdateCategory called for id: {Id}", id);
			UpdateCategoryCommand command = new([mapper.ToDomain(model)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateCategory called for id: {Id}, but not found", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(UpdateCategory), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update categories in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> UpdateCategories([FromBody] List<UpdateCategoryRequest> models)
	{
		try
		{
			logger.LogDebug("UpdateCategories called with {Count} categories", models.Count);
			UpdateCategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("UpdateCategories called with {Count} categories, but not found", models.Count);
				return NotFound();
			}

			logger.LogDebug("UpdateCategories called with {Count} categories, and found", models.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(UpdateCategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete categories")]
	[EndpointDescription("Deletes one or more categories by their IDs. Returns 404 if any category is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<bool>> DeleteCategories([FromBody] List<Guid> ids)
	{
		try
		{
			logger.LogDebug("DeleteCategories called with {Count} ids", ids.Count);
			DeleteCategoryCommand command = new(ids);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("DeleteCategories called with {Count} ids, but not found", ids.Count);
				return NotFound();
			}

			logger.LogDebug("DeleteCategories called with {Count} ids, and found", ids.Count);

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(DeleteCategories));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted category")]
	[EndpointDescription("Restores a previously soft-deleted category by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> RestoreCategory([FromRoute] Guid id)
	{
		try
		{
			logger.LogDebug("RestoreCategory called with id: {Id}", id);
			RestoreCategoryCommand command = new(id);
			bool result = await mediator.Send(command);

			if (!result)
			{
				logger.LogWarning("RestoreCategory called with id: {Id}, but not found or not deleted", id);
				return NotFound();
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithId, nameof(RestoreCategory), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
