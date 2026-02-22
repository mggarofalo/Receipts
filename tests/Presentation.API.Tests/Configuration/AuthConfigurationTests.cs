using API.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Presentation.API.Tests.Configuration;

public class AuthConfigurationTests
{
	private static IOptionsMonitor<JwtBearerOptions> BuildJwtOptionsMonitor()
	{
		ServiceCollection services = new();
		services.AddLogging();

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>())
			.Build();

		services.AddAuthServices(configuration);

		ServiceProvider sp = services.BuildServiceProvider();
		return sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
	}

	private static MessageReceivedContext CreateMessageReceivedContext(JwtBearerOptions options, string path, string queryString = "")
	{
		DefaultHttpContext httpContext = new();
		httpContext.Request.Path = path;
		httpContext.Request.QueryString = new QueryString(queryString);

		AuthenticationScheme scheme = new(
			JwtBearerDefaults.AuthenticationScheme,
			null,
			typeof(JwtBearerHandler));

		return new MessageReceivedContext(httpContext, scheme, options);
	}

	// ── OnMessageReceived ──────────────────────────────────────────────────

	[Fact]
	public async Task OnMessageReceived_SetsToken_WhenPathIsHubAndAccessTokenPresent()
	{
		// Arrange
		JwtBearerOptions options = BuildJwtOptionsMonitor().Get(JwtBearerDefaults.AuthenticationScheme);
		MessageReceivedContext context = CreateMessageReceivedContext(options, "/hubs/receipts", "?access_token=my-jwt");

		// Act
		await options.Events.MessageReceived(context);

		// Assert
		Assert.Equal("my-jwt", context.Token);
	}

	[Fact]
	public async Task OnMessageReceived_SetsToken_WhenPathIsHubNegotiate()
	{
		// Arrange – SignalR negotiate also hits the hub path prefix
		JwtBearerOptions options = BuildJwtOptionsMonitor().Get(JwtBearerDefaults.AuthenticationScheme);
		MessageReceivedContext context = CreateMessageReceivedContext(
			options, "/hubs/receipts/negotiate", "?access_token=negotiate-token");

		// Act
		await options.Events.MessageReceived(context);

		// Assert
		Assert.Equal("negotiate-token", context.Token);
	}

	[Fact]
	public async Task OnMessageReceived_DoesNotSetToken_WhenPathIsNotHub()
	{
		// Arrange
		JwtBearerOptions options = BuildJwtOptionsMonitor().Get(JwtBearerDefaults.AuthenticationScheme);
		MessageReceivedContext context = CreateMessageReceivedContext(
			options, "/api/receipts", "?access_token=should-not-apply");

		// Act
		await options.Events.MessageReceived(context);

		// Assert
		Assert.Null(context.Token);
	}

	[Fact]
	public async Task OnMessageReceived_DoesNotSetToken_WhenAccessTokenQueryParamIsMissing()
	{
		// Arrange
		JwtBearerOptions options = BuildJwtOptionsMonitor().Get(JwtBearerDefaults.AuthenticationScheme);
		MessageReceivedContext context = CreateMessageReceivedContext(options, "/hubs/receipts");

		// Act
		await options.Events.MessageReceived(context);

		// Assert
		Assert.Null(context.Token);
	}

	[Fact]
	public async Task OnMessageReceived_DoesNotSetToken_WhenAccessTokenIsEmpty()
	{
		// Arrange
		JwtBearerOptions options = BuildJwtOptionsMonitor().Get(JwtBearerDefaults.AuthenticationScheme);
		MessageReceivedContext context = CreateMessageReceivedContext(options, "/hubs/receipts", "?access_token=");

		// Act
		await options.Events.MessageReceived(context);

		// Assert
		Assert.Null(context.Token);
	}

	// ── Access-token log redaction ─────────────────────────────────────────
	// Validates the regex that protects JWTs from appearing in Serilog request logs.

	private static string RedactQueryString(string queryString)
	{
		return Regex.Replace(queryString, @"(?i)access_token=[^&]*", "access_token=[REDACTED]");
	}

	[Fact]
	public void LogRedaction_ReplacesAccessTokenValue_InQueryString()
	{
		string result = RedactQueryString("?access_token=secret.jwt.value");
		Assert.Equal("?access_token=[REDACTED]", result);
	}

	[Fact]
	public void LogRedaction_PreservesOtherParameters_WhenRedacting()
	{
		string result = RedactQueryString("?foo=bar&access_token=secret&baz=qux");
		Assert.Equal("?foo=bar&access_token=[REDACTED]&baz=qux", result);
	}

	[Fact]
	public void LogRedaction_IsCaseInsensitive()
	{
		string result = RedactQueryString("?ACCESS_TOKEN=secret.jwt.value");
		Assert.Equal("?access_token=[REDACTED]", result);
	}

	[Fact]
	public void LogRedaction_LeavesQueryStringUnchanged_WhenNoAccessToken()
	{
		const string queryString = "?foo=bar&baz=qux";
		string result = RedactQueryString(queryString);
		Assert.Equal(queryString, result);
	}
}
