using API.Generated.Dtos;
using API.Mapping.Core;
using API.Services;
using Application.Commands.Card.Create;
using Application.Commands.Card.Delete;
using Application.Commands.Card.Update;
using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Core.Card;
using Asp.Versioning;
using Domain.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/cards")]
[Produces("application/json")]
[Authorize]
public class CardsController(IMediator mediator, CardMapper mapper, ILogger<CardsController> logger, IEntityChangeNotifier notifier, ICardService cardService) : ControllerBase
{
	public const string RouteGetById = "{id}";
	public const string RouteGetAll = "";
	public const string RouteCreate = "";
	public const string RouteCreateBatch = "batch";
	public const string RouteUpdate = "{id}";
	public const string RouteUpdateBatch = "batch";
	public const string RouteDelete = "{id}";

	[HttpGet(RouteGetById)]
	[EndpointSummary("Get a card by ID")]
	[EndpointDescription("Returns a single card matching the provided GUID.")]
	public async Task<Results<Ok<CardResponse>, NotFound>> GetCardById([FromRoute] Guid id)
	{
		GetCardByIdQuery query = new(id);
		Card? result = await mediator.Send(query);

		if (result == null)
		{
			logger.LogWarning("Card {Id} not found", id);
			return TypedResults.NotFound();
		}

		CardResponse model = mapper.ToResponse(result);
		return TypedResults.Ok(model);
	}

	[HttpGet(RouteGetAll)]
	[EndpointSummary("Get all cards")]
	public async Task<Results<Ok<CardListResponse>, BadRequest<string>>> GetAllCards([FromQuery] bool? isActive = null, [FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
	{
		if (offset < 0)
		{
			return TypedResults.BadRequest("offset must be >= 0");
		}

		if (limit <= 0 || limit > 500)
		{
			return TypedResults.BadRequest("limit must be between 1 and 500");
		}

		if (sortBy is not null && !SortableColumns.Card.Contains(sortBy))
		{
			return TypedResults.BadRequest($"Invalid sortBy '{sortBy}'. Allowed: {string.Join(", ", SortableColumns.Card)}");
		}

		if (!SortableColumns.IsValidDirection(sortDirection))
		{
			return TypedResults.BadRequest($"Invalid sortDirection '{sortDirection}'. Allowed: asc, desc");
		}

		SortParams sort = new(sortBy, sortDirection);
		GetAllCardsQuery query = new(offset, limit, sort, isActive);
		PagedResult<Card> result = await mediator.Send(query);

		return TypedResults.Ok(new CardListResponse
		{
			Data = [.. result.Data.Select(mapper.ToResponse)],
			Total = result.Total,
			Offset = result.Offset,
			Limit = result.Limit,
		});
	}

	[HttpPost(RouteCreate)]
	[EndpointSummary("Create a single card")]
	public async Task<Ok<CardResponse>> CreateCard([FromBody] CreateCardRequest model)
	{
		CreateCardCommand command = new([mapper.ToDomain(model)]);
		List<Card> cards = await mediator.Send(command);
		await notifier.NotifyCreated("card", cards[0].Id);
		return TypedResults.Ok(mapper.ToResponse(cards[0]));
	}

	[HttpPost(RouteCreateBatch)]
	[EndpointSummary("Create cards in batch")]
	public async Task<Ok<List<CardResponse>>> CreateCards([FromBody] List<CreateCardRequest> models)
	{
		CreateCardCommand command = new([.. models.Select(mapper.ToDomain)]);
		List<Card> cards = await mediator.Send(command);
		await notifier.NotifyBulkChanged("card", "created", cards.Select(a => a.Id));
		return TypedResults.Ok(cards.Select(mapper.ToResponse).ToList());
	}

	[HttpPut(RouteUpdate)]
	[EndpointSummary("Update a single card")]
	public async Task<Results<NoContent, NotFound>> UpdateCard([FromRoute] Guid id, [FromBody] UpdateCardRequest model)
	{
		UpdateCardCommand command = new([mapper.ToDomain(model)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Card {Id} not found for update", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyUpdated("card", id);
		return TypedResults.NoContent();
	}

	[HttpPut(RouteUpdateBatch)]
	[EndpointSummary("Update cards in batch")]
	public async Task<Results<NoContent, NotFound>> UpdateCards([FromBody] List<UpdateCardRequest> models)
	{
		UpdateCardCommand command = new([.. models.Select(mapper.ToDomain)]);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Cards batch update failed — not found");
			return TypedResults.NotFound();
		}

		await notifier.NotifyBulkChanged("card", "updated", models.Select(m => m.Id));
		return TypedResults.NoContent();
	}

	[HttpDelete(RouteDelete)]
	[Authorize(Policy = "RequireAdmin")]
	[EndpointSummary("Hard-delete a card")]
	[EndpointDescription("Permanently deletes a card. Requires the Admin role. Returns 409 Conflict if transactions reference this card.")]
	public async Task<Results<NoContent, NotFound, Conflict<object>>> DeleteCard([FromRoute] Guid id)
	{
		int transactionCount = await cardService.GetTransactionCountByCardIdAsync(id, HttpContext.RequestAborted);
		if (transactionCount > 0)
		{
			logger.LogWarning("Card {Id} cannot be deleted — {Count} transactions reference it", id, transactionCount);
			return TypedResults.Conflict<object>(new { message = $"Cannot delete — {transactionCount} transaction(s) reference this card", transactionCount });
		}

		DeleteCardCommand command = new(id);
		bool result = await mediator.Send(command);

		if (!result)
		{
			logger.LogWarning("Card {Id} not found for deletion", id);
			return TypedResults.NotFound();
		}

		await notifier.NotifyDeleted("card", id);
		return TypedResults.NoContent();
	}
}
