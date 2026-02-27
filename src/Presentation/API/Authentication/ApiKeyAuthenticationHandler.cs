using Application.Interfaces.Services;
using Infrastructure.Entities;
using Infrastructure.Entities.Audit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace API.Authentication;

public static class ApiKeyAuthenticationDefaults
{
	public const string AuthenticationScheme = "ApiKey";
}

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions { }

public class ApiKeyAuthenticationHandler(
	IOptionsMonitor<ApiKeyAuthenticationOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	IApiKeyService apiKeyService,
	IAuthAuditService authAuditService,
	UserManager<ApplicationUser> userManager)
	: AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
	private const string ApiKeyHeader = "X-API-Key";

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.TryGetValue(ApiKeyHeader, out Microsoft.Extensions.Primitives.StringValues apiKeyValues))
		{
			return AuthenticateResult.NoResult();
		}

		string? apiKey = apiKeyValues.FirstOrDefault();
		if (string.IsNullOrWhiteSpace(apiKey))
		{
			return AuthenticateResult.NoResult();
		}

		string? userId = await apiKeyService.GetUserIdByApiKeyAsync(apiKey);
		if (userId is null)
		{
			return AuthenticateResult.Fail("Invalid API key.");
		}

		List<Claim> claims = [new Claim(ClaimTypes.NameIdentifier, userId)];

		ApplicationUser? user = await userManager.FindByIdAsync(userId);
		if (user is not null)
		{
			if (user.Email is not null)
			{
				claims.Add(new Claim(ClaimTypes.Email, user.Email));
			}

			IList<string> roles = await userManager.GetRolesAsync(user);
			foreach (string role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}
		}

		ClaimsIdentity identity = new(claims, Scheme.Name);
		ClaimsPrincipal principal = new(identity);
		AuthenticationTicket ticket = new(principal, Scheme.Name);

		try
		{
			await authAuditService.LogAsync(new AuthAuditEntryDto(
				Guid.NewGuid(),
				nameof(AuthEventType.ApiKeyUsed),
				userId,
				null,
				user?.Email,
				true,
				null,
				Context.Connection.RemoteIpAddress?.ToString(),
				Request.Headers.UserAgent.ToString(),
				DateTimeOffset.UtcNow,
				null));
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to log API key usage audit event");
		}

		return AuthenticateResult.Success(ticket);
	}
}
