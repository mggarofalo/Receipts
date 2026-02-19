using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
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
	IApiKeyService apiKeyService)
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

		Claim[] claims = [new Claim(ClaimTypes.NameIdentifier, userId)];
		ClaimsIdentity identity = new(claims, Scheme.Name);
		ClaimsPrincipal principal = new(identity);
		AuthenticationTicket ticket = new(principal, Scheme.Name);

		return AuthenticateResult.Success(ticket);
	}
}
