using System.Security.Claims;
using API.Controllers;
using Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class AuthAuditControllerTests
{
	private readonly Mock<IAuthAuditService> _authAuditServiceMock;
	private readonly AuthAuditController _controller;

	public AuthAuditControllerTests()
	{
		_authAuditServiceMock = new Mock<IAuthAuditService>();
		_controller = new AuthAuditController(_authAuditServiceMock.Object);
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
		Results<Ok<List<AuthAuditEntryDto>>, UnauthorizedHttpResult> result = await _controller.GetMyAuditLog(50, CancellationToken.None);

		// Assert
		Ok<List<AuthAuditEntryDto>> okResult = Assert.IsType<Ok<List<AuthAuditEntryDto>>>(result.Result);
		List<AuthAuditEntryDto> returnedLogs = okResult.Value!;
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
		Ok<List<AuthAuditEntryDto>> result = await _controller.GetRecent(50, CancellationToken.None);

		// Assert
		List<AuthAuditEntryDto> returnedLogs = result.Value!;
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
		Ok<List<AuthAuditEntryDto>> result = await _controller.GetFailed(50, CancellationToken.None);

		// Assert
		List<AuthAuditEntryDto> returnedLogs = result.Value!;
		returnedLogs.Should().HaveCount(1);
		returnedLogs[0].Success.Should().BeFalse();
	}

	[Fact]
	public async Task GetMyAuditLog_ThrowsException_WhenServiceFails()
	{
		// Arrange
		string userId = "user-123";
		SetupUserClaims(userId);
		_authAuditServiceMock
			.Setup(s => s.GetMyAuditLogAsync(userId, 50, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		Func<Task> act = () => _controller.GetMyAuditLog(50, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetRecent_ThrowsException_WhenServiceFails()
	{
		// Arrange
		_authAuditServiceMock
			.Setup(s => s.GetRecentAsync(50, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		Func<Task> act = () => _controller.GetRecent(50, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetFailed_ThrowsException_WhenServiceFails()
	{
		// Arrange
		_authAuditServiceMock
			.Setup(s => s.GetFailedAttemptsAsync(50, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		Func<Task> act = () => _controller.GetFailed(50, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}
}
