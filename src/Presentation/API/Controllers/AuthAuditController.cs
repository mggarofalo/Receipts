using System.Security.Claims;
using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/auth/audit")]
[Produces("application/json")]
[Authorize]
public class AuthAuditController(IAuthAuditService authAuditService) : ControllerBase
{
	[HttpGet("me")]
	public async Task<Results<Ok<List<AuthAuditEntryDto>>, UnauthorizedHttpResult>> GetMyAuditLog([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return TypedResults.Unauthorized();
		}

		List<AuthAuditEntryDto> logs = await authAuditService.GetMyAuditLogAsync(userId, count, cancellationToken);
		return TypedResults.Ok(logs);
	}

	[HttpGet("recent")]
	public async Task<Ok<List<AuthAuditEntryDto>>> GetRecent([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		List<AuthAuditEntryDto> logs = await authAuditService.GetRecentAsync(count, cancellationToken);
		return TypedResults.Ok(logs);
	}

	[HttpGet("failed")]
	public async Task<Ok<List<AuthAuditEntryDto>>> GetFailed([FromQuery] int count = 50, CancellationToken cancellationToken = default)
	{
		List<AuthAuditEntryDto> logs = await authAuditService.GetFailedAttemptsAsync(count, cancellationToken);
		return TypedResults.Ok(logs);
	}
}
