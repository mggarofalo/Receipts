using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Subcategory.Create;
using Application.Commands.Subcategory.Delete;
using Application.Commands.Subcategory.Update;
using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Core.Subcategory;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/subcategories")]
[Produces("application/json")]
[Authorize]
public class SubcategoriesController(IMediator mediator, SubcategoryMapper mapper, ILogger<SubcategoriesController> logger, IEntityChangeNotifier notifier, ISubcategoryService subcategoryService) : ControllerBase
{
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteCreateBatch = "batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "{id}";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a subcategory by ID")]
	[EndpointDescription("Returns a single subcategory matching the provided GUID.")]
	public async Task<Results<Ok<SubcategoryResponse>, NotFound>> GetSubcategoryById([FromRoute] Guid id)
	{
		GetSubcategoryByIdQuery query = new(id);
		Subcategory? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Subcategory {Id} not found", id);
			return TypedResults.NotFound();
		}

		SubcategoryResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all subcategories")]
	public async Task<Results<Ok<SubcategoryListResponse>, BadRequest<string>>> GetAllSubcategories([FromQuery] Guid? categoryId = null, [FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
	{
		if (offset < 0)
		{
			return TypedResults.BadRequest("offset must be >= 0");
		}

		if (limit <= 0 || limit > 500)
		{
			return TypedResults.BadRequest("limit must be between 1 and 500");
		}

		if (sortBy is not null && !SortableColumns.Subcategory.Contains(sortBy))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sortBy}'. Allowed: {string.Join(", ", SortableColumns.Subcategory)}");
		}

		if (!SortableColumns.IsValidDirection(sortDirection))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{sortDirection}'. Allowed: asc, desc");
		}

		SortParams sort = new(sortBy, sortDirection);

		if (categoryId.HasValue)
		{
			GetSubcategoriesByCategoryIdQuery byCategoryQuery = new(categoryId.Value, offset, limit, sort);
			PagedResult<Subcategory> byCategoryResult = await mediator.Send(byCategoryQuery);

			return TypedResults.Ok(new SubcategoryListResponse
			{
				Data = [.. byCategoryResult.Data.Select(mapper.ToResponse)],
				Total = byCategoryResult.Total,
				Offset = byCategoryResult.Offset,
				Limit = byCategoryResult.Limit,
			});
		}

		GetAllSubcategoriesQuery query = new(offset, limit, sort);
		PagedResult<Subcategory> result = await mediator.Send(query);

		return TypedResults.Ok(new SubcategoryListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single subcategory")]
	public async Task<Ok<SubcategoryResponse>> CreateSubcategory([FromBody] CreateSubcategoryRequest model)
	{
		CreateSubcategoryCommand command = new([mapper.ToDomain(model)]);
		List<Subcategory> subcategories = await mediator.Send(command);
		await notifier.NotifyCreated("subcategory", subcategories[0].Id);
		return TypedResults.Ok(mapper.ToResponse(subcategories[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create subcategories in batch")]
	public async Task<Ok<List<SubcategoryResponse>>> CreateSubcategories([FromBody] List<CreateSubcategoryRequest> models)
	{
		CreateSubcategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Subcategory> subcategories = await mediator.Send(command);
		await notifier.NotifyBulkChanged("subcategory", "created", subcategories.Select(s => s.Id));
		return TypedResults.Ok(subcategories.Select(mapper.ToResponse).ToList());
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single subcategory")]
	public async Task<Results<NoContent, NotFound>> UpdateSubcategory([FromRoute] Guid id, [FromBody] UpdateSubcategoryRequest model)
	{
		UpdateSubcategoryCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Subcategory {Id} not found for update", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("subcategory", id);
		return TypedResults.NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update subcategories in batch")]
	public async Task<Results<NoContent, NotFound>> UpdateSubcategories([FromBody] List<UpdateSubcategoryRequest> models)
	{
		UpdateSubcategoryCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Subcategories batch update failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("subcategory", "updated", models.Select(m => m.Id));
		return TypedResults.NoContent();
	}

	[HttpDelete(RouteDelete)]
	[Authorize(Policy = "RequireAdmin")]
	[EndpointSummary("Hard-delete a subcategory")]
	[EndpointDescription("Permanently deletes a subcategory. Requires the Admin role. Returns 409 Conflict if receipt items reference this subcategory.")]
	public async Task<Results<NoContent, NotFound, Conflict<object>>> DeleteSubcategory([FromRoute] Guid id)
	{
		Subcategory? subcategory = await mediator.Send(new GetSubcategoryByIdQuery(id));
		if (subcategory == null)
		{
			logger.LogWarning("Subcategory {Id} not found for deletion", id);
			return TypedResults.NotFound();
		}

		int receiptItemCount = await subcategoryService.GetReceiptItemCountBySubcategoryNameAsync(subcategory.Name, HttpContext.RequestAborted);
		if (receiptItemCount > 0)
		{
			logger.LogWarning("Subcategory {Id} cannot be deleted — {Count} receipt items reference it", id, receiptItemCount);
			return TypedResults.Conflict<object>(new { message = $"Cannot delete — {receiptItemCount} receipt item(s) use this subcategory", receiptItemCount });
		}

		DeleteSubcategoryCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Subcategory {Id} not found for deletion", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyDeleted("subcategory", id);
		return TypedResults.NoContent();
	}
}
