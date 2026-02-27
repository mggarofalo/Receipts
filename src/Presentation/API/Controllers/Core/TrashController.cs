using Application.Commands.Trash.Purge;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core;

[ApiVersion("1.0")]
[ApiController]
[Route("api/trash")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class TrashController(IMediator mediator, ILogger<TrashController> logger) : ControllerBase
{
	[HttpPost("purge")]
	[EndpointSummary("Permanently delete all soft-deleted items")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> PurgeTrash()
	{
		try
		{
			logger.LogWarning("PurgeTrash called — permanently deleting all soft-deleted items");
			PurgeTrashCommand command = new();
			await mediator.Send(command);
			return NoContent();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error in {Method}", nameof(PurgeTrash));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}
}
