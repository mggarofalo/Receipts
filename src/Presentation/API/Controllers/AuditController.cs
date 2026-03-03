using API.Generated.Dtos;
using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/audit")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AuditController(IAuditService auditService) : ControllerBase
{
	[HttpGet("")]
	[ProducesResponseType<List<AuditLogResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetAuditLogs(
		[FromQuery] string? entityType = null,
		[FromQuery] string? entityId = null,
		[FromQuery] string? userId = null,
		[FromQuery] Guid? apiKeyId = null,
		CancellationToken cancellationToken = default)
	{
		if (entityType != null && entityId != null)
		{
			List<AuditLogDto> entityLogs = await auditService.GetByEntityAsync(entityType, entityId, cancellationToken);
			return Ok(entityLogs);
		}

		if (userId != null)
		{
			List<AuditLogDto> userLogs = await auditService.GetByUserAsync(userId, cancellationToken);
			return Ok(userLogs);
		}

		if (apiKeyId.HasValue)
		{
			List<AuditLogDto> apiKeyLogs = await auditService.GetByApiKeyAsync(apiKeyId.Value, cancellationToken);
			return Ok(apiKeyLogs);
		}

		return BadRequest("At least one filter parameter (entityType+entityId, userId, or apiKeyId) is required.");
	}

	[HttpGet("recent")]
	[ProducesResponseType<List<AuditLogResponse>>(StatusCodes.Status200OK)]
	public async Task<IActionResult> GetRecent([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		List<AuditLogDto> logs = await auditService.GetRecentAsync(count, cancellationToken);
		return Ok(logs);
	}
}
