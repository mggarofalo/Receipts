using API.Services;
using Application.Commands.Trash.Purge;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/trash")]
[Produces("application/json")]
[Authorize]
public class TrashController(IMediator mediator, ILogger<TrashController> logger, IEntityChangeNotifier notifier) : ControllerBase
{
	[HttpPost("purge")]
	[EndpointSummary("Permanently delete all soft-deleted items")]
	public async Task<NoContent> PurgeTrash()
	{
		logger.LogWarning("PurgeTrash called — permanently deleting all soft-deleted items");
		PurgeTrashCommand command = new();
		await mediator.Send(command);

		string[] entityTypes = ["receipt", "receipt-item", "transaction", "adjustment", "account", "category", "subcategory", "item-template"];
		foreach (string entityType in entityTypes)
		{
			await notifier.NotifyAllChanged(entityType, "deleted");
		}

		return TypedResults.NoContent();
	}
}
