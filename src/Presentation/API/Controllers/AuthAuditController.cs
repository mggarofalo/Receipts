using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/auth/audit")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AuthAuditController(IAuthAuditService authAuditService, ILogger<AuthAuditController> logger) : ControllerBase
{
	public const string MessageWithoutId = "Error occurred in {Method}";

	[HttpGet("me")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetMyAuditLog([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		try
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId is null)
			{
				return Unauthorized();
			}

			List<AuthAuditEntryDto> logs = await authAuditService.GetMyAuditLogAsync(userId, count, cancellationToken);
			return Ok(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetMyAuditLog));
			return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
		}
	}

	[HttpGet("recent")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetRecent([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		try
		{
			List<AuthAuditEntryDto> logs = await authAuditService.GetRecentAsync(count, cancellationToken);
			return Ok(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetRecent));
			return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
		}
	}

	[HttpGet("failed")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetFailed([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		try
		{
			List<AuthAuditEntryDto> logs = await authAuditService.GetFailedAttemptsAsync(count, cancellationToken);
			return Ok(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, MessageWithoutId, nameof(GetFailed));
			return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
		}
	}
}
