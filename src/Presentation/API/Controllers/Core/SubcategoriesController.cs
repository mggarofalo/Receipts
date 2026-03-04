using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Subcategory.Create;
using Application.Commands.Subcategory.Delete;
using Application.Commands.Subcategory.Restore;
using Application.Commands.Subcategory.Update;
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
public class SubcategoriesController(IMediator mediator, SubcategoryMapper mapper, ILogger<SubcategoriesController> logger, IEntityChangeNotifier notifier) : ControllerBase
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
	public async Task<Ok<SubcategoryListResponse>> GetAllSubcategories([FromQuery] Guid? categoryId = null, [FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		if (categoryId.HasValue)
		{
			GetSubcategoriesByCategoryIdQuery byCategoryQuery = new(categoryId.Value, offset, limit);
			PagedResult<Subcategory> byCategoryResult = await mediator.Send(byCategoryQuery);

			return TypedResults.Ok(new SubcategoryListResponse
			{
				Data = [.. byCategoryResult.Data.Select(mapper.ToResponse)],
				Total = byCategoryResult.Total,
				Offset = byCategoryResult.Offset,
				Limit = byCategoryResult.Limit,
			});
		}

		GetAllSubcategoriesQuery query = new(offset, limit);
		PagedResult<Subcategory> result = await mediator.Send(query);

		return TypedResults.Ok(new SubcategoryListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpGet(RouteGetDeleted)]
	[EndpointSummary("Get all soft-deleted subcategories")]
	[EndpointDescription("Returns all subcategories that have been soft-deleted.")]
	public async Task<Ok<SubcategoryListResponse>> GetDeletedSubcategories([FromQuery] int offset = 0, [FromQuery] int limit = 50)
	{
		GetDeletedSubcategoriesQuery query = new(offset, limit);
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
	[EndpointSummary("Delete subcategories")]
	[EndpointDescription("Deletes one or more subcategories by their IDs. Returns 404 if any subcategory is not found.")]
	public async Task<Results<NoContent, NotFound>> DeleteSubcategories([FromBody] List<Guid> ids)
	{
		DeleteSubcategoryCommand command = new(ids);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Subcategories delete failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("subcategory", "deleted", ids);
		return TypedResults.NoContent();
	}

	[HttpPost(RouteRestore)]
	[EndpointSummary("Restore a soft-deleted subcategory")]
	[EndpointDescription("Restores a previously soft-deleted subcategory by clearing its DeletedAt timestamp.")]
	public async Task<Results<NoContent, NotFound>> RestoreSubcategory([FromRoute] Guid id)
	{
		RestoreSubcategoryCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Subcategory {Id} not found or not deleted for restore", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("subcategory", id);
		return TypedResults.NoContent();
	}
}
