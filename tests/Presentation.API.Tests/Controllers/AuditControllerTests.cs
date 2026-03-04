using API.Controllers;
using Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class AuditControllerTests
{
	private readonly Mock<IAuditService> _auditServiceMock;
	private readonly AuditController _controller;

	public AuditControllerTests()
	{
		_auditServiceMock = new Mock<IAuditService>();
		_controller = new AuditController(_auditServiceMock.Object);
	}

	[Fact]
	public async Task GetAuditLogs_WithEntityParams_ReturnsOk_WithAuditLogs()
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
		IActionResult result = await _controller.GetAuditLogs(entityType, entityId, null, null, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetAuditLogs_WithEntityParams_NoLogs_ReturnsOk_EmptyList()
	{
		// Arrange
		string entityType = "Account";
		string entityId = Guid.NewGuid().ToString();

		_auditServiceMock.Setup(s => s.GetByEntityAsync(entityType, entityId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		// Act
		IActionResult result = await _controller.GetAuditLogs(entityType, entityId, null, null, CancellationToken.None);

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
	public async Task GetAuditLogs_WithUserId_ReturnsOk_WithUserLogs()
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
		IActionResult result = await _controller.GetAuditLogs(null, null, userId, null, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
		returnedLogs[0].ChangedByUserId.Should().Be(userId);
	}

	[Fact]
	public async Task GetAuditLogs_WithApiKeyId_ReturnsOk_WithApiKeyLogs()
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
		IActionResult result = await _controller.GetAuditLogs(null, null, null, apiKeyId, CancellationToken.None);

		// Assert
		OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
		List<AuditLogDto> returnedLogs = Assert.IsType<List<AuditLogDto>>(okResult.Value);
		returnedLogs.Should().HaveCount(1);
		returnedLogs[0].ChangedByApiKeyId.Should().Be(apiKeyId);
	}

	[Fact]
	public async Task GetAuditLogs_WithEntityParams_ThrowsException_WhenServiceFails()
	{
		// Arrange
		string entityType = "Account";
		string entityId = Guid.NewGuid().ToString();

		_auditServiceMock.Setup(s => s.GetByEntityAsync(entityType, entityId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		Func<Task> act = () => _controller.GetAuditLogs(entityType, entityId, null, null, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetRecent_ThrowsException_WhenServiceFails()
	{
		// Arrange
		_auditServiceMock.Setup(s => s.GetRecentAsync(50, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		Func<Task> act = () => _controller.GetRecent(50, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetAuditLogs_WithUserId_ThrowsException_WhenServiceFails()
	{
		// Arrange
		string userId = "test-user";
		_auditServiceMock.Setup(s => s.GetByUserAsync(userId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		Func<Task> act = () => _controller.GetAuditLogs(null, null, userId, null, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task GetAuditLogs_WithApiKeyId_ThrowsException_WhenServiceFails()
	{
		// Arrange
		Guid apiKeyId = Guid.NewGuid();
		_auditServiceMock.Setup(s => s.GetByApiKeyAsync(apiKeyId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Test error"));

		// Act
		Func<Task> act = () => _controller.GetAuditLogs(null, null, null, apiKeyId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
	}
}
