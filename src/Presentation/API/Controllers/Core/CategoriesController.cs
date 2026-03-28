using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Category.Create;
using Application.Commands.Category.Delete;
using Application.Commands.Category.Update;
using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Core.Category;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
[Authorize]
public class CategoriesController(IMediator mediator, CategoryMapper mapper, ILogger<CategoriesController> logger, IEntityChangeNotifier notifier, ICategoryService categoryService) : ControllerBase
{
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteCreateBatch = "batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "{id}";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a category by ID")]
	[EndpointDescription("Returns a single category matching the provided GUID.")]
	public async Task<Results<Ok<CategoryResponse>, NotFound>> GetCategoryById([FromRoute] Guid id)
	{
		GetCategoryByIdQuery query = new(id);
		Category? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Category {Id} not found", id);
			return TypedResults.NotFound();
		}

		CategoryResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all categories")]
	public async Task<Results<Ok<CategoryListResponse>, BadRequest<string>>> GetAllCategories([FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
	{
		if (offset < 0)
		{
			return TypedResults.BadRequest("offset must be >= 0");
		}

		if (limit <= 0 || limit > 500)
		{
			return TypedResults.BadRequest("limit must be between 1 and 500");
		}

		if (sortBy is not null && !SortableColumns.Category.Contains(sortBy))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sortBy}'. Allowed: {string.Join(", ", SortableColumns.Category)}");
		}

		if (!SortableColumns.IsValidDirection(sortDirection))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{sortDirection}'. Allowed: asc, desc");
		}

		SortParams sort = new(sortBy, sortDirection);
		GetAllCategoriesQuery query = new(offset, limit, sort);
		PagedResult<Category> result = await mediator.Send(query);

		return TypedResults.Ok(new CategoryListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single category")]
	public async Task<Ok<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest model)
	{
		CreateCategoryCommand command = new([mapper.ToDomain(model)]);
		List<Category> categories = await mediator.Send(command);
		await notifier.NotifyCreated("category", categories[0].Id);
		return TypedResults.Ok(mapper.ToResponse(categories[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create categories in batch")]
	public async Task<Ok<List<CategoryResponse>>> CreateCategories([FromBody] List<CreateCategoryRequest> models)
	{
		CreateCategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Category> categories = await mediator.Send(command);
		await notifier.NotifyBulkChanged("category", "created", categories.Select(c => c.Id));
		return TypedResults.Ok(categories.Select(mapper.ToResponse).ToList());
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single category")]
	public async Task<Results<NoContent, NotFound>> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryRequest model)
	{
		UpdateCategoryCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Category {Id} not found for update", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("category", id);
		return TypedResults.NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update categories in batch")]
	public async Task<Results<NoContent, NotFound>> UpdateCategories([FromBody] List<UpdateCategoryRequest> models)
	{
		UpdateCategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Categories batch update failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("category", "updated", models.Select(m => m.Id));
		return TypedResults.NoContent();
	}

	[HttpDelete(RouteDelete)]
	[Authorize(Policy = "RequireAdmin")]
	[EndpointSummary("Hard-delete a category")]
	[EndpointDescription("Permanently deletes a category. Requires the Admin role. Returns 409 Conflict if subcategories or receipt items reference this category.")]
	public async Task<Results<NoContent, NotFound, Conflict<object>>> DeleteCategory([FromRoute] Guid id)
	{
		Category? category = await mediator.Send(new GetCategoryByIdQuery(id));
		if (category == null)
		{
			logger.LogWarning("Category {Id} not found for deletion", id);
			return TypedResults.NotFound();
		}

		int subcategoryCount = await categoryService.GetSubcategoryCountAsync(id, HttpContext.RequestAborted);
		if (subcategoryCount > 0)
		{
			logger.LogWarning("Category {Id} cannot be deleted — {Count} subcategories reference it", id, subcategoryCount);
			return TypedResults.Conflict<object>(new { message = $"Cannot delete — {subcategoryCount} subcategories belong to this category. Delete them first.", subcategoryCount });
		}

		int receiptItemCount = await categoryService.GetReceiptItemCountByCategoryNameAsync(category.Name, HttpContext.RequestAborted);
		if (receiptItemCount > 0)
		{
			logger.LogWarning("Category {Id} cannot be deleted — {Count} receipt items reference it", id, receiptItemCount);
			return TypedResults.Conflict<object>(new { message = $"Cannot delete — {receiptItemCount} receipt item(s) use this category", receiptItemCount });
		}

		DeleteCategoryCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Category {Id} not found for deletion", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyDeleted("category", id);
		return TypedResults.NoContent();
	}
}
