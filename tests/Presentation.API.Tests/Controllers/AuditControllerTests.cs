using API.Controllers;
using Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class AuditControllerTests
{
	private readonly Mock<IAuditService> _auditServiceMock;
	private readonly Mock<ILogger<AuditController>> _loggerMock;
	private readonly AuditController _controller;

	public AuditControllerTests()
	{
		_auditServiceMock = new Mock<IAuditService>();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<AuditController>();
		_controller = new AuditController(_auditServiceMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task GetEntityHistory_ReturnsOk_WithAuditLogs()
	{
		// Arrange
		string entityType = "Account";
		string entityId = Guid.NewGuid().ToString();
		List<AuditLogDto> logs =
		[
			new AuditLogDto(Guid.NewGuid(), entityType, entityId, "Create", "[]", null, null, DateTimeOffset.UtcNow, null)
		];

		_auditServiceMock.Setup(s => s.GetByEntityAsync(entityType, entityId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(logs);

		// Act
		IActionResult result = await _controller.GetEntityHistory(entityType, entityId, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetEntityHistory_NoLogs_ReturnsOk_EmptyList()
	{
		// Arrange
		_auditServiceMock.Setup(s => s.GetByEntityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		// Act
		IActionResult result = await _controller.GetEntityHistory("Account", Guid.NewGuid().ToString(), CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().BeEmpty();
	}

	[Fact]
	public async Task GetRecent_ReturnsOk_WithRecentLogs()
	{
		// Arrange
		List<AuditLogDto> logs =
		[
			new AuditLogDto(Guid.NewGuid(), "Account", Guid.NewGuid().ToString(), "Create", "[]", null, null, DateTimeOffset.UtcNow, null),
			new AuditLogDto(Guid.NewGuid(), "Receipt", Guid.NewGuid().ToString(), "Update", "[]", null, null, DateTimeOffset.UtcNow, null)
		];

		_auditServiceMock.Setup(s => s.GetRecentAsync(50, It.IsAny<CancellationToken>()))
			.ReturnsAsync(logs);

		// Act
		IActionResult result = await _controller.GetRecent(50, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByUser_ReturnsOk_WithUserLogs()
	{
		// Arrange
		string userId = "test-user";
		List<AuditLogDto> logs =
		[
			new AuditLogDto(Guid.NewGuid(), "Account", Guid.NewGuid().ToString(), "Create", "[]", userId, null, DateTimeOffset.UtcNow, null)
		];

		_auditServiceMock.Setup(s => s.GetByUserAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(logs);

		// Act
		IActionResult result = await _controller.GetByUser(userId, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
		returnedLogs[0].ChangedByUserId.Should().Be(userId);
	}

	[Fact]
	public async Task GetByApiKey_ReturnsOk_WithApiKeyLogs()
	{
		// Arrange
		Guid apiKeyId = Guid.NewGuid();
		List<AuditLogDto> logs =
		[
			new AuditLogDto(Guid.NewGuid(), "Account", Guid.NewGuid().ToString(), "Create", "[]", null, apiKeyId, DateTimeOffset.UtcNow, null)
		];

		_auditServiceMock.Setup(s => s.GetByApiKeyAsync(apiKeyId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(logs);

		// Act
		IActionResult result = await _controller.GetByApiKey(apiKeyId, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
		returnedLogs[0].ChangedByApiKeyId.Should().Be(apiKeyId);
	}

	[Fact]
	public async Task GetEntityHistory_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		_auditServiceMock.Setup(s => s.GetByEntityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		IActionResult result = await _controller.GetEntityHistory("Account", Guid.NewGuid().ToString(), CancellationToken.None);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetRecent_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		_auditServiceMock.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		IActionResult result = await _controller.GetRecent(50, CancellationToken.None);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetByUser_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		_auditServiceMock.Setup(s => s.GetByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		IActionResult result = await _controller.GetByUser("test-user", CancellationToken.None);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
		Assert.Equal(500, objectResult.StatusCode);
	}

	[Fact]
	public async Task GetByApiKey_ReturnsInternalServerError_WhenExceptionThrown()
	{
		// Arrange
		_auditServiceMock.Setup(s => s.GetByApiKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		IActionResult result = await _controller.GetByApiKey(Guid.NewGuid(), CancellationToken.None);

		// Assert
		ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
		Assert.Equal(500, objectResult.StatusCode);
	}
}
