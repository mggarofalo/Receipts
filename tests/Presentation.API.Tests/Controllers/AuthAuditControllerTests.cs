using API.Controllers;
using Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Presentation.API.Tests.Controllers;

public class AuthAuditControllerTests
{
	private readonly Mock<IAuthAuditService> _authAuditServiceMock;
	private readonly Mock<ILogger<AuthAuditController>> _loggerMock;
	private readonly AuthAuditController _controller;

	public AuthAuditControllerTests()
	{
		_authAuditServiceMock = new Mock<IAuthAuditService>();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<AuthAuditController>();
		_controller = new AuthAuditController(_authAuditServiceMock.Object, _loggerMock.Object);
	}

	private void SetupUserClaims(string userId)
	{
		List<Claim> claims = [new Claim(ClaimTypes.NameIdentifier, userId)];
		ClaimsIdentity identity = new(claims, "TestAuth");
		ClaimsPrincipal principal = new(identity);
		_controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext { User = principal }
		};
	}

	[Fact]
	public async Task GetMyAuditLog_ReturnsOk_WithUserAuthEvents()
	{
		// Arrange
		string userId = "user-123";
		SetupUserClaims(userId);
		List<AuthAuditEntryDto> logs =
		[
			new AuthAuditEntryDto(Guid.NewGuid(), "Login", userId, null, "testuser@example.com", true, null, "192.168.1.1", "TestAgent/1.0", DateTimeOffset.UtcNow, null)
		];

		_authAuditServiceMock
			.Setup(s => s.GetMyAuditLogAsync(userId, 50, It.IsAny<CancellationToken>()))
			.ReturnsAsync(logs);

		// Act
		IActionResult result = await _controller.GetMyAuditLog(50, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuthAuditEntryDto> returnedLogs = Assert.IsType<List<AuthAuditEntryDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
		returnedLogs[0].UserId.Should().Be(userId);
	}

	[Fact]
	public async Task GetRecent_ReturnsOk_WithRecentAuthEvents()
	{
		// Arrange
		List<AuthAuditEntryDto> logs =
		[
			new AuthAuditEntryDto(Guid.NewGuid(), "Login", "user-1", null, "user1@example.com", true, null, "10.0.0.1", "Agent/1.0", DateTimeOffset.UtcNow, null),
			new AuthAuditEntryDto(Guid.NewGuid(), "LoginFailed", "user-2", null, "user2@example.com", false, "Bad password", "10.0.0.2", "Agent/2.0", DateTimeOffset.UtcNow, null)
		];

		_authAuditServiceMock
			.Setup(s => s.GetRecentAsync(50, It.IsAny<CancellationToken>()))
			.ReturnsAsync(logs);

		// Act
		IActionResult result = await _controller.GetRecent(50, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuthAuditEntryDto> returnedLogs = Assert.IsType<List<AuthAuditEntryDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetFailed_ReturnsOk_WithFailedAttempts()
	{
		// Arrange
		List<AuthAuditEntryDto> logs =
		[
			new AuthAuditEntryDto(Guid.NewGuid(), "LoginFailed", "user-1", null, "user1@example.com", false, "Invalid password", "10.0.0.1", "Agent/1.0", DateTimeOffset.UtcNow, null)
		];

		_authAuditServiceMock
			.Setup(s => s.GetFailedAttemptsAsync(50, It.IsAny<CancellationToken>()))
			.ReturnsAsync(logs);

		// Act
		IActionResult result = await _controller.GetFailed(50, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuthAuditEntryDto> returnedLogs = Assert.IsType<List<AuthAuditEntryDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
		returnedLogs[0].Success.Should().BeFalse();
	}

	[Fact]
	public async Task GetMyAuditLog_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		SetupUserClaims("user-123");
		_authAuditServiceMock
			.Setup(s => s.GetMyAuditLogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		IActionResult result = await _controller.GetMyAuditLog(50, CancellationToken.None);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetRecent_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		_authAuditServiceMock
			.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		IActionResult result = await _controller.GetRecent(50, CancellationToken.None);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetFailed_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		_authAuditServiceMock
			.Setup(s => s.GetFailedAttemptsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		IActionResult result = await _controller.GetFailed(50, CancellationToken.None);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
		Assert.Equal(500, objectResult.StatusCode);
	}
}
