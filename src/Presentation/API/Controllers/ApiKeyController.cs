using API.Generated.Dtos;
using Application.Interfaces.Services;
using Infrastructure.Entities.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/apikeys")]
[Produces("application/json")]
[Authorize]
public class ApiKeyController(
	IApiKeyService apiKeyService,
	IAuthAuditService authAuditService,
	ILogger<ApiKeyController> logger) : ControllerBase
{
	[HttpGet]
	[EndpointSummary("Get all API keys for the authenticated user")]
	[ProducesResponseType<List<ApiKeyResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<List<ApiKeyResponse>>> GetApiKeys()
	{
		try
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId is null)
			{
				return Unauthorized();
			}

			IReadOnlyList<ApiKeyInfo> keys = await apiKeyService.GetApiKeysForUserAsync(userId);
			List<ApiKeyResponse> response = [.. keys.Select(k => new ApiKeyResponse
			{
				Id = k.Id,
				Name = k.Name,
				CreatedAt = k.CreatedAt,
				LastUsedAt = k.LastUsedAt,
				ExpiresAt = k.ExpiresAt,
				IsRevoked = k.IsRevoked,
			})];

			return Ok(response);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(GetApiKeys));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost]
	[EndpointSummary("Create a new API key")]
	[ProducesResponseType<CreateApiKeyResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<CreateApiKeyResponse>> CreateApiKey([FromBody] CreateApiKeyRequest request)
	{
		try
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId is null)
			{
				return Unauthorized();
			}

			string rawKey = await apiKeyService.CreateApiKeyAsync(userId, request.Name, request.ExpiresAt);

			IReadOnlyList<ApiKeyInfo> keys = await apiKeyService.GetApiKeysForUserAsync(userId);
			ApiKeyInfo? created = keys.OrderByDescending(k => k.CreatedAt).FirstOrDefault(k => k.Name == request.Name);

			await LogAuthEventAsync(nameof(AuthEventType.ApiKeyCreated), userId, null, true, null, created?.Id);

			return Ok(new CreateApiKeyResponse
			{
				Id = created?.Id ?? Guid.Empty,
				Name = request.Name,
				RawKey = rawKey,
				CreatedAt = created?.CreatedAt ?? DateTimeOffset.UtcNow,
				ExpiresAt = request.ExpiresAt,
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method}", nameof(CreateApiKey));
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpDelete("{id}")]
	[EndpointSummary("Revoke an API key")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> RevokeApiKey([FromRoute] Guid id)
	{
		try
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId is null)
			{
				return Unauthorized();
			}

			await apiKeyService.RevokeApiKeyAsync(id, userId);
			await LogAuthEventAsync(nameof(AuthEventType.ApiKeyRevoked), userId, null, true, null, id);
			return NoContent();
		}
		catch (KeyNotFoundException)
		{
			return NotFound();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error occurred in {Method} for id: {Id}", nameof(RevokeApiKey), id);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	private async Task LogAuthEventAsync(string eventType, string? userId, string? username, bool success, string? failureReason = null, Guid? apiKeyId = null)
	{
		try
		{
			await authAuditService.LogAsync(new AuthAuditEntryDto(
				Guid.NewGuid(),
				eventType,
				userId,
				apiKeyId,
				username,
				success,
				failureReason,
				HttpContext.Connection.RemoteIpAddress?.ToString(),
				Request.Headers.UserAgent.ToString(),
				DateTimeOffset.UtcNow,
				null));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to log auth audit event {EventType}", eventType);
		}
	}
}
