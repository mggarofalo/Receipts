using System.Net;
using System.Security.Claims;
using API.Configuration;
using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Presentation.API.Tests.Configuration;

/// <summary>
/// Integration tests that verify the full auth + rate limiting pipeline works end-to-end.
/// These test the actual <see cref="AuthConfiguration.UseAuthServices"/> method with a
/// simulated API key identity to verify that rate limit bypass works through the real
/// middleware pipeline (not in isolation). Unlike <see cref="RateLimitingConfigurationTests"/>,
/// these tests exercise the actual middleware ordering.
/// </summary>
[Trait("Category", "Integration")]
public class RateLimitBypassIntegrationTests
{
	private static readonly Dictionary<string, string?> TestConfig = new()
	{
		// Very low rate limits to make bypass observable
		["RateLimiting:Global:PermitLimit"] = "2",
		["RateLimiting:Global:WindowMinutes"] = "1",
		["RateLimiting:Global:SegmentsPerWindow"] = "4",
		["RateLimiting:Auth:PermitLimit"] = "2",
		["RateLimiting:Auth:WindowMinutes"] = "1",
		["RateLimiting:AuthSensitive:PermitLimit"] = "2",
		["RateLimiting:AuthSensitive:WindowMinutes"] = "1",
		["RateLimiting:ApiKey:PermitLimit"] = "2",
		["RateLimiting:ApiKey:WindowMinutes"] = "1",
		// JWT config (required by AddAuthServices)
		["Jwt:Key"] = "test-key-that-is-at-least-32-characters-long-for-hmac-sha256",
		["Jwt:Issuer"] = "test-issuer",
		["Jwt:Audience"] = "test-audience",
	};

	[Fact]
	public async Task ApiKeyWithBypass_ExceedsGlobalLimit_AllRequestsSucceed()
	{
		// Arrange: Full pipeline with a bypass API key
		using IHost host = CreateHostWithFullPipeline(bypassRateLimit: true);
		await host.StartAsync();
		HttpClient client = host.GetTestClient();

		// Act: Send more requests than the global rate limit (permit = 2)
		List<HttpResponseMessage> responses = [];
		for (int i = 0; i < 5; i++)
		{
			responses.Add(await client.GetAsync("/api/test"));
		}

		// Assert: All requests should succeed because the BypassRateLimit claim
		// was available to the rate limiter (authorization ran before rate limiting)
		responses.Should().AllSatisfy(r =>
			r.StatusCode.Should().Be(HttpStatusCode.OK,
				"API key with BypassRateLimit=true should bypass the global rate limit"));
	}

	[Fact]
	public async Task ApiKeyWithoutBypass_ExceedsGlobalLimit_Gets429()
	{
		// Arrange: Full pipeline with a non-bypass API key
		using IHost host = CreateHostWithFullPipeline(bypassRateLimit: false);
		await host.StartAsync();
		HttpClient client = host.GetTestClient();

		// Act: Send more requests than the global rate limit (permit = 2)
		List<HttpResponseMessage> responses = [];
		for (int i = 0; i < 5; i++)
		{
			responses.Add(await client.GetAsync("/api/test"));
		}

		// Assert: First 2 should succeed, then rate limiting kicks in
		responses.Take(2).Should().AllSatisfy(r =>
			r.StatusCode.Should().Be(HttpStatusCode.OK));
		responses.Skip(2).Should().Contain(r =>
			r.StatusCode == HttpStatusCode.TooManyRequests,
			"API key without BypassRateLimit should be rate limited");
	}

	[Fact]
	public async Task AnonymousRequest_ExceedsGlobalLimit_Gets429()
	{
		// Arrange: Full pipeline with no authentication
		using IHost host = CreateHostWithFullPipeline(bypassRateLimit: null);
		await host.StartAsync();
		HttpClient client = host.GetTestClient();

		// Act: Send more requests than the global rate limit (permit = 2)
		List<HttpResponseMessage> responses = [];
		for (int i = 0; i < 5; i++)
		{
			responses.Add(await client.GetAsync("/api/test-anon"));
		}

		// Assert: First 2 should succeed, then rate limiting kicks in
		responses.Take(2).Should().AllSatisfy(r =>
			r.StatusCode.Should().Be(HttpStatusCode.OK));
		responses.Skip(2).Should().Contain(r =>
			r.StatusCode == HttpStatusCode.TooManyRequests,
			"Anonymous requests should still be rate limited");
	}

	private static IHost CreateHostWithFullPipeline(bool? bypassRateLimit)
	{
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(TestConfig)
			.Build();

		WebApplicationBuilder appBuilder = WebApplication.CreateBuilder();
		appBuilder.WebHost.UseTestServer();

		// Register the real auth services (JWT + ApiKey schemes + authorization policies)
		appBuilder.Services.AddAuthServices(configuration);

		// Register the real rate limiting services
		appBuilder.Services.AddApplicationServices(configuration);

		// Register mock dependencies required by ApiKeyAuthenticationHandler
		appBuilder.Services.AddSingleton(new Mock<IApiKeyService>().Object);
		appBuilder.Services.AddSingleton(new Mock<IAuthAuditService>().Object);
		appBuilder.Services.AddSingleton(CreateMockUserManager());

		WebApplication app = appBuilder.Build();

		// Inject a test identity that simulates what ApiKeyAuthenticationHandler would set.
		// This runs BEFORE the auth pipeline so context.User is populated before
		// UseAuthentication/UseAuthorization/UseRateLimiter.
		if (bypassRateLimit.HasValue)
		{
			app.Use(async (context, next) =>
			{
				ClaimsIdentity identity = new("TestApiKey");
				identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "test-user-id"));
				identity.AddClaim(new Claim("BypassRateLimit",
					bypassRateLimit.Value.ToString().ToLowerInvariant()));
				context.User = new ClaimsPrincipal(identity);
				await next();
			});
		}

		// Use the actual auth pipeline under test — this is the code we're verifying
		app.UseAuthServices();

		// Test endpoint — AllowAnonymous so authorization doesn't reject, but the full
		// auth pipeline still runs. The key test is whether the rate limiter sees the
		// BypassRateLimit claim that was set before the pipeline.
		app.MapGet("/api/test", () => Results.Ok("OK"))
			.AllowAnonymous();

		// Separate anonymous endpoint for the anonymous rate limiting test
		app.MapGet("/api/test-anon", () => Results.Ok("OK"))
			.AllowAnonymous();

		return app;
	}

	private static UserManager<ApplicationUser> CreateMockUserManager()
	{
		Mock<IUserStore<ApplicationUser>> store = new();
		return new UserManager<ApplicationUser>(
			store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
	}
}
