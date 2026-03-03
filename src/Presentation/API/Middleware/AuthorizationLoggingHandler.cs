using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace API.Middleware;

public class AuthorizationLoggingHandler(ILogger<AuthorizationLoggingHandler> logger) : IAuthorizationMiddlewareResultHandler
{
	private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

	public async Task HandleAsync(
		RequestDelegate next,
		HttpContext context,
		AuthorizationPolicy policy,
		PolicyAuthorizationResult authorizeResult)
	{
		if (authorizeResult.Forbidden)
		{
			string? userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			logger.LogWarning(
				"Authorization denied for user {UserId}. Method: {Method}, Path: {Path}, IP: {IP}",
				userId, context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);
		}

		await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
	}
}
