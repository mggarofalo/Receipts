using API.Generated.Dtos;
using API.Mapping.Core;
using Application.Commands.Category.Create;
using Application.Commands.Category.Delete;
using Application.Commands.Category.Restore;
using Application.Commands.Category.Update;
using Application.Models;
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
	public async Task<ActionResult<CategoryResponse>> GetCategoryById([FromRoute] Guid id)
	{
		GetCategoryByIdQuery query = new(id);
		Category? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Category {Id} not found", id);
			return NotFound();
		}

		CategoryResponse model = mapper.ToResponse(result);
		return Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all categories")]
	[ProducesResponseType<CategoryListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<CategoryListResponse>> GetAllCategories([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetAllCategoriesQuery query = new(offset, limit);
		PagedResult<Category> result = await mediator.Send(query);

		return Ok(new CategoryListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted categories")]
	[EndpointDescription("Returns all categories that have been soft-deleted.")]
	[ProducesResponseType<CategoryListResponse>(StatusCodes.Status200OK)]
	public async Task<ActionResult<CategoryListResponse>> GetDeletedCategories([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedCategoriesQuery query = new(offset, limit);
		PagedResult<Category> result = await mediator.Send(query);

		return Ok(new CategoryListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single category")]
	[ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest model)
	{
		CreateCategoryCommand command = new([mapper.ToDomain(model)]);
		List<Category> categories = await mediator.Send(command);
		return Ok(mapper.ToResponse(categories[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create categories in batch")]
	[ProducesResponseType<List<CategoryResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<ActionResult<List<CategoryResponse>>> CreateCategories([FromBody] List<CreateCategoryRequest> models)
	{
		CreateCategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Category> categories = await mediator.Send(command);
		return Ok(categories.Select(mapper.ToResponse).ToList());
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single category")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryRequest model)
	{
		UpdateCategoryCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Category {Id} not found for update", id);
			return NotFound();
		}

		return NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update categories in batch")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> UpdateCategories([FromBody] List<UpdateCategoryRequest> models)
	{
		UpdateCategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Categories batch update failed — not found");
			return NotFound();
		}

		return NoContent();
	}

	[HttpDelete(RouteDelete)]
	[EndpointSummary("Delete categories")]
	[EndpointDescription("Deletes one or more categories by their IDs. Returns 404 if any category is not found.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<bool>> DeleteCategories([FromBody] List<Guid> ids)
	{
		DeleteCategoryCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Categories delete failed — not found");
			return NotFound();
		}

		return NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted category")]
	[EndpointDescription("Restores a previously soft-deleted category by clearing its DeletedAt timestamp.")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RestoreCategory([FromRoute] Guid id)
	{
		RestoreCategoryCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Category {Id} not found or not deleted for restore", id);
			return NotFound();
		}

		return NoContent();
	}
}
