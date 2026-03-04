using System.Security.Claims;
using API.Generated.Dtos;
using Application.Interfaces.Services;
using Asp.Versioning;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
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
	public async Task<Results<Ok<List<ApiKeyResponse>>, UnauthorizedHttpResult>> GetApiKeys()
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return TypedResults.Unauthorized();
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

		return TypedResults.Ok(response);
	}

	[HttpPost]
	[EndpointSummary("Create a new API key")]
	public async Task<Results<Ok<CreateApiKeyResponse>, UnauthorizedHttpResult>> CreateApiKey([FromBody] CreateApiKeyRequest request)
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return TypedResults.Unauthorized();
		}

		string rawKey = await apiKeyService.CreateApiKeyAsync(userId, request.Name, request.ExpiresAt);

		IReadOnlyList<ApiKeyInfo> keys = await apiKeyService.GetApiKeysForUserAsync(userId);
		ApiKeyInfo? created = keys.OrderByDescending(k => k.CreatedAt).FirstOrDefault(k => k.Name == request.Name);

		await LogAuthEventAsync(nameof(AuthEventType.ApiKeyCreated), userId, null, true, null, created?.Id);

		return TypedResults.Ok(new CreateApiKeyResponse
		{
			Id = created?.Id ?? Guid.Empty,
			Name = request.Name,
			RawKey = rawKey,
			CreatedAt = created?.CreatedAt ?? DateTimeOffset.UtcNow,
			ExpiresAt = request.ExpiresAt,
		});
	}

	[HttpDelete("{id}")]
	[EndpointSummary("Revoke an API key")]
	public async Task<Results<NoContent, UnauthorizedHttpResult, NotFound>> RevokeApiKey([FromRoute] Guid id)
	{
		string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId is null)
		{
			return TypedResults.Unauthorized();
		}

		await apiKeyService.RevokeApiKeyAsync(id, userId);
		await LogAuthEventAsync(nameof(AuthEventType.ApiKeyRevoked), userId, null, true, null, id);
		return TypedResults.NoContent();
	}

	private async Task LogAuthEventAsync(string eventType, string? userId, string? username, bool success, string? failureReason = null, Guid? apiKeyId = null)
	{
		try
		{
			await authAuditService.LogAsync(new Application.Interfaces.Services.AuthAuditEntryDto(
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
