using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/audit")]
[Produces("application/json")]
[Authorize]
public class AuditController(IAuditService auditService) : ControllerBase
{
	[HttpGet("")]
	public async Task<Results<Ok<List<AuditLogDto>>, BadRequest<string>>> GetAuditLogs(
		[FromQuery] string? entityType = null,
		[FromQuery] string? entityId = null,
		[FromQuery] string? userId = null,
		[FromQuery] Guid? apiKeyId = null,
		CancellationToken cancellationToken = default)
	{
		if (entityType != null && entityId != null)
		{
			List<AuditLogDto> entityLogs = await auditService.GetByEntityAsync(entityType, entityId, cancellationToken);
			return TypedResults.Ok(entityLogs);
		}

		if (userId != null)
		{
			List<AuditLogDto> userLogs = await auditService.GetByUserAsync(userId, cancellationToken);
			return TypedResults.Ok(userLogs);
		}

		if (apiKeyId.HasValue)
		{
			List<AuditLogDto> apiKeyLogs = await auditService.GetByApiKeyAsync(apiKeyId.Value, cancellationToken);
			return TypedResults.Ok(apiKeyLogs);
		}

		return TypedResults.BadRequest("At least one filter parameter (entityType+entityId, userId, or apiKeyId) is required.");
	}

	[HttpGet("recent")]
	public async Task<Ok<List<AuditLogDto>>> GetRecent([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		List<AuditLogDto> logs = await auditService.GetRecentAsync(count, cancellationToken);
		return TypedResults.Ok(logs);
	}
}
