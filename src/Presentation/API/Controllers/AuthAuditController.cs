using System.Security.Claims;
using API.Generated.Dtos;
using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/auth/audit")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AuthAuditController(IAuthAuditService authAuditService) : ControllerBase
{
	[HttpGet("me")]
	[ProducesResponseType<List<AuthAuditLogResponse>>(StatusCodes.Status200OK)]
	public async Task<IActionResult> GetMyAuditLog([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return Unauthorized();
		}

		List<AuthAuditEntryDto> logs = await authAuditService.GetMyAuditLogAsync(userId, count, cancellationToken);
		return Ok(logs);
	}

	[HttpGet("recent")]
	[ProducesResponseType<List<AuthAuditLogResponse>>(StatusCodes.Status200OK)]
	public async Task<IActionResult> GetRecent([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		List<AuthAuditEntryDto> logs = await authAuditService.GetRecentAsync(count, cancellationToken);
		return Ok(logs);
	}

	[HttpGet("failed")]
	[ProducesResponseType<List<AuthAuditLogResponse>>(StatusCodes.Status200OK)]
	public async Task<IActionResult> GetFailed([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		List<AuthAuditEntryDto> logs = await authAuditService.GetFailedAttemptsAsync(count, cancellationToken);
		return Ok(logs);
	}
}
