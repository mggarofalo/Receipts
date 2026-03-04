using System.Security.Claims;
using API.Controllers;
using API.Generated.Dtos;
using Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class ApiKeyControllerTests
{
	private readonly Mock<IApiKeyService> _apiKeyServiceMock;
	private readonly Mock<IAuthAuditService> _authAuditServiceMock;
	private readonly Mock<ILogger<ApiKeyController>> _loggerMock;
	private readonly ApiKeyController _controller;

	public ApiKeyControllerTests()
	{
		_apiKeyServiceMock = new Mock<IApiKeyService>();
		_authAuditServiceMock = new Mock<IAuthAuditService>();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<ApiKeyController>();

		_controller = new ApiKeyController(
			_apiKeyServiceMock.Object,
			_authAuditServiceMock.Object,
			_loggerMock.Object);

		_controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};
	}

	private void SetupUserClaims(string userId)
	{
		List<Claim> claims = [new Claim(ClaimTypes.NameIdentifier, userId)];
		ClaimsIdentity identity = new(claims, "TestAuth");
		ClaimsPrincipal principal = new(identity);
		_controller.ControllerContext.HttpContext.User = principal;
	}

	// ── GetApiKeys ──────────────────────────────────────────

	[Fact]
	public async Task GetApiKeys_ReturnsOk_WhenAuthenticated()
	{
		SetupUserClaims("user-123");
		List<ApiKeyInfo> keys =
		[
			new(Guid.NewGuid(), "key-1", DateTimeOffset.UtcNow, null, null, false),
			new(Guid.NewGuid(), "key-2", DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow.AddDays(30), false),
		];
		_apiKeyServiceMock.Setup(s => s.GetApiKeysForUserAsync("user-123", It.IsAny<CancellationToken>())).ReturnsAsync(keys);

		Results<Ok<List<ApiKeyResponse>>, UnauthorizedHttpResult> result = await _controller.GetApiKeys();

		Ok<List<ApiKeyResponse>> okResult = Assert.IsType<Ok<List<ApiKeyResponse>>>(result.Result);
		okResult.Value.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetApiKeys_ReturnsUnauthorized_WhenNoClaims()
	{
		Results<Ok<List<ApiKeyResponse>>, UnauthorizedHttpResult> result = await _controller.GetApiKeys();

		Assert.IsType<UnauthorizedHttpResult>(result.Result);
	}

	// ── CreateApiKey ────────────────────────────────────────

	[Fact]
	public async Task CreateApiKey_ReturnsOk_WhenAuthenticated()
	{
		SetupUserClaims("user-123");
		Guid keyId = Guid.NewGuid();
		DateTimeOffset createdAt = DateTimeOffset.UtcNow;

		_apiKeyServiceMock.Setup(s => s.CreateApiKeyAsync("user-123", "my-key", null, It.IsAny<CancellationToken>()))
			.ReturnsAsync("raw-key-value");
		_apiKeyServiceMock.Setup(s => s.GetApiKeysForUserAsync("user-123", It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<ApiKeyInfo> { new(keyId, "my-key", createdAt, null, null, false) });

		Results<Ok<CreateApiKeyResponse>, UnauthorizedHttpResult> result = await _controller.CreateApiKey(
			new CreateApiKeyRequest { Name = "my-key" });

		Ok<CreateApiKeyResponse> okResult = Assert.IsType<Ok<CreateApiKeyResponse>>(result.Result);
		okResult.Value!.RawKey.Should().Be("raw-key-value");
		okResult.Value!.Name.Should().Be("my-key");
	}

	[Fact]
	public async Task CreateApiKey_ReturnsUnauthorized_WhenNoClaims()
	{
		Results<Ok<CreateApiKeyResponse>, UnauthorizedHttpResult> result = await _controller.CreateApiKey(
			new CreateApiKeyRequest { Name = "my-key" });

		Assert.IsType<UnauthorizedHttpResult>(result.Result);
	}

	// ── RevokeApiKey ────────────────────────────────────────

	[Fact]
	public async Task RevokeApiKey_ReturnsNoContent_WhenAuthenticated()
	{
		SetupUserClaims("user-123");
		Guid keyId = Guid.NewGuid();

		Results<NoContent, UnauthorizedHttpResult> result = await _controller.RevokeApiKey(keyId);

		Assert.IsType<NoContent>(result.Result);
		_apiKeyServiceMock.Verify(s => s.RevokeApiKeyAsync(keyId, "user-123", It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RevokeApiKey_ReturnsUnauthorized_WhenNoClaims()
	{
		Results<NoContent, UnauthorizedHttpResult> result = await _controller.RevokeApiKey(Guid.NewGuid());

		Assert.IsType<UnauthorizedHttpResult>(result.Result);
	}
}
