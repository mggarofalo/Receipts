using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.Tests.Services;

public class AuthAuditCleanupServiceTests
{
	private readonly Mock<IAuthAuditService> _auditServiceMock;
	private readonly Mock<ILogger<AuthAuditCleanupService>> _loggerMock;
	private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
	private readonly Mock<IServiceScope> _scopeMock;
	private readonly Mock<IServiceProvider> _serviceProviderMock;

	public AuthAuditCleanupServiceTests()
	{
		_auditServiceMock = new Mock<IAuthAuditService>();
		_loggerMock = new Mock<ILogger<AuthAuditCleanupService>>();
		_scopeFactoryMock = new Mock<IServiceScopeFactory>();
		_scopeMock = new Mock<IServiceScope>();
		_serviceProviderMock = new Mock<IServiceProvider>();

		_serviceProviderMock
			.Setup(sp => sp.GetService(typeof(IAuthAuditService)))
			.Returns(_auditServiceMock.Object);
		_scopeMock
			.Setup(s => s.ServiceProvider)
			.Returns(_serviceProviderMock.Object);
		_scopeFactoryMock
			.Setup(f => f.CreateScope())
			.Returns(_scopeMock.Object);
	}

	[Fact]
	public async Task ExecuteAsync_CallsCleanupOnService()
	{
		// Arrange
		TaskCompletionSource executed = new();
		_auditServiceMock
			.Setup(s => s.CleanupOldEntriesAsync(180, It.IsAny<CancellationToken>()))
			.Returns<int, CancellationToken>((_, _) =>
			{
				executed.TrySetResult();
				return Task.FromResult(5);
			});

		AuthAuditCleanupService service = new(_scopeFactoryMock.Object, _loggerMock.Object);
		using CancellationTokenSource cts = new();

		// Act — StartAsync dispatches ExecuteAsync to the thread pool via Task.Run,
		// so we must wait for the TCS before cancelling to avoid a race.
		await service.StartAsync(cts.Token);
		await executed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		cts.Cancel();
		await service.StopAsync(CancellationToken.None);

		// Assert
		_auditServiceMock.Verify(
			s => s.CleanupOldEntriesAsync(180, It.IsAny<CancellationToken>()),
			Times.AtLeastOnce());
	}

	[Fact]
	public async Task ExecuteAsync_LogsCleanupStatistics()
	{
		// Arrange
		TaskCompletionSource executed = new();
		_auditServiceMock
			.Setup(s => s.CleanupOldEntriesAsync(180, It.IsAny<CancellationToken>()))
			.Returns<int, CancellationToken>((_, _) =>
			{
				executed.TrySetResult();
				return Task.FromResult(42);
			});

		AuthAuditCleanupService service = new(_scopeFactoryMock.Object, _loggerMock.Object);
		using CancellationTokenSource cts = new();

		// Act
		await service.StartAsync(cts.Token);
		await executed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		cts.Cancel();
		await service.StopAsync(CancellationToken.None);

		// Assert
		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.AtLeastOnce());
	}

	[Fact]
	public async Task ExecuteAsync_ContinuesOnException()
	{
		// Arrange
		TaskCompletionSource executed = new();
		_auditServiceMock
			.Setup(s => s.CleanupOldEntriesAsync(180, It.IsAny<CancellationToken>()))
			.Returns<int, CancellationToken>((_, _) =>
			{
				executed.TrySetResult();
				return Task.FromException<int>(new InvalidOperationException("Database error"));
			});

		AuthAuditCleanupService service = new(_scopeFactoryMock.Object, _loggerMock.Object);
		using CancellationTokenSource cts = new();

		// Act — should not throw
		await service.StartAsync(cts.Token);
		await executed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		cts.Cancel();
		Func<Task> act = async () => await service.StopAsync(CancellationToken.None);

		// Assert
		await act.Should().NotThrowAsync();
		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.AtLeastOnce());
	}
}
