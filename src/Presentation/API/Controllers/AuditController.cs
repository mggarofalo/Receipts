using API.Generated.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/audit")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AuditController(IAuditService auditService, ILogger<AuditController> logger) : ControllerBase
{
	public const string MessageWithoutId = "Error occurred in {Method}";

	[HttpGet("entity/{entityType}/{entityId}")]
	[ProducesResponseType<List<AuditLogResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetEntityHistory(string entityType, string entityId, CancellationToken cancellationToken)
	{
		try
		{
			List<AuditLogDto> logs = await auditService.GetByEntityAsync(entityType, entityId, cancellationToken);
			return Ok(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetEntityHistory));
			return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
		}
	}

	[HttpGet("recent")]
	[ProducesResponseType<List<AuditLogResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetRecent([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		try
		{
			List<AuditLogDto> logs = await auditService.GetRecentAsync(count, cancellationToken);
			return Ok(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetRecent));
			return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
		}
	}

	[HttpGet("user/{userId}")]
	[ProducesResponseType<List<AuditLogResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetByUser(string userId, CancellationToken cancellationToken)
	{
		try
		{
			List<AuditLogDto> logs = await auditService.GetByUserAsync(userId, cancellationToken);
			return Ok(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetByUser));
			return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
		}
	}

	[HttpGet("apikey/{apiKeyId}")]
	[ProducesResponseType<List<AuditLogResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetByApiKey(Guid apiKeyId, CancellationToken cancellationToken)
	{
		try
		{
			List<AuditLogDto> logs = await auditService.GetByApiKeyAsync(apiKeyId, cancellationToken);
			return Ok(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetByApiKey));
			return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
		}
	}
}
