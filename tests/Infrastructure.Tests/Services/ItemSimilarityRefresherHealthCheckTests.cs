using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Infrastructure.Tests.Services;

public class ItemSimilarityRefresherHealthCheckTests
{
	private static readonly DateTimeOffset ReferenceTime = new(2026, 4, 19, 12, 0, 0, TimeSpan.Zero);

	private static (ItemSimilarityEdgeRefresher Refresher, FakeTimeProvider TimeProvider) CreateRefresher()
	{
		FakeTimeProvider timeProvider = new(ReferenceTime);
		Mock<IServiceScopeFactory> scopeFactory = new();
		Mock<IDescriptionChangeSignal> signal = new();
		ItemSimilarityEdgeRefresher refresher = new(
			scopeFactory.Object,
			signal.Object,
			NullLogger<ItemSimilarityEdgeRefresher>.Instance,
			timeProvider);
		return (refresher, timeProvider);
	}

	private static Task<HealthCheckResult> RunAsync(ItemSimilarityEdgeRefresher refresher, FakeTimeProvider timeProvider)
	{
		ItemSimilarityRefresherHealthCheck check = new(refresher, timeProvider);
		HealthCheckContext context = new()
		{
			Registration = new HealthCheckRegistration("item_similarity_refresher", check, null, null),
		};
		return check.CheckHealthAsync(context);
	}

	[Fact]
	public async Task NeverRefreshed_IsUnhealthy()
	{
		// Arrange
		(ItemSimilarityEdgeRefresher refresher, FakeTimeProvider timeProvider) = CreateRefresher();

		// Act
		HealthCheckResult result = await RunAsync(refresher, timeProvider);

		// Assert
		result.Status.Should().Be(HealthStatus.Unhealthy);
		result.Description.Should().Contain("has not completed a successful refresh");
		result.Data["consecutiveFailures"].Should().Be(0);
		result.Data["lastSuccessfulRefreshAt"].Should().Be("never");
	}

	[Fact]
	public async Task RecentSuccess_IsHealthy()
	{
		// Arrange
		(ItemSimilarityEdgeRefresher refresher, FakeTimeProvider timeProvider) = CreateRefresher();
		refresher.LastSuccessfulRefreshAt = ReferenceTime.AddMinutes(-5);

		// Act
		HealthCheckResult result = await RunAsync(refresher, timeProvider);

		// Assert
		result.Status.Should().Be(HealthStatus.Healthy);
	}

	[Fact]
	public async Task ThreeConsecutiveFailures_IsUnhealthy_EvenWithRecentSuccess()
	{
		// Arrange
		(ItemSimilarityEdgeRefresher refresher, FakeTimeProvider timeProvider) = CreateRefresher();
		refresher.LastSuccessfulRefreshAt = ReferenceTime.AddMinutes(-1);
		refresher.ConsecutiveFailures = ItemSimilarityEdgeRefresher.MaxConsecutiveFailures;

		// Act
		HealthCheckResult result = await RunAsync(refresher, timeProvider);

		// Assert
		result.Status.Should().Be(HealthStatus.Unhealthy);
		result.Description.Should().Contain("consecutive failures");
		result.Data["consecutiveFailures"].Should().Be(ItemSimilarityEdgeRefresher.MaxConsecutiveFailures);
	}

	[Fact]
	public async Task RecentSuccessWithTransientFailure_IsHealthy()
	{
		// Arrange — a single failure after a recent success must not fail the check.
		(ItemSimilarityEdgeRefresher refresher, FakeTimeProvider timeProvider) = CreateRefresher();
		refresher.LastSuccessfulRefreshAt = ReferenceTime.AddMinutes(-1);
		refresher.ConsecutiveFailures = 1;

		// Act
		HealthCheckResult result = await RunAsync(refresher, timeProvider);

		// Assert
		result.Status.Should().Be(HealthStatus.Healthy);
		result.Data["consecutiveFailures"].Should().Be(1);
	}

	[Fact]
	public async Task StaleSuccess_IsUnhealthy()
	{
		// Arrange — last success older than MaxIdleInterval + slack should fail.
		(ItemSimilarityEdgeRefresher refresher, FakeTimeProvider timeProvider) = CreateRefresher();
		TimeSpan threshold = ItemSimilarityEdgeRefresher.MaxIdleInterval + ItemSimilarityRefresherHealthCheck.StalenessSlack;
		refresher.LastSuccessfulRefreshAt = ReferenceTime - threshold - TimeSpan.FromMinutes(1);

		// Act
		HealthCheckResult result = await RunAsync(refresher, timeProvider);

		// Assert
		result.Status.Should().Be(HealthStatus.Unhealthy);
		result.Description.Should().Contain("last succeeded");
	}

	[Fact]
	public async Task SuccessExactlyAtThreshold_IsHealthy()
	{
		// Arrange — boundary case: last success right at threshold must still be Healthy.
		(ItemSimilarityEdgeRefresher refresher, FakeTimeProvider timeProvider) = CreateRefresher();
		TimeSpan threshold = ItemSimilarityEdgeRefresher.MaxIdleInterval + ItemSimilarityRefresherHealthCheck.StalenessSlack;
		refresher.LastSuccessfulRefreshAt = ReferenceTime - threshold;

		// Act
		HealthCheckResult result = await RunAsync(refresher, timeProvider);

		// Assert
		result.Status.Should().Be(HealthStatus.Healthy);
	}
}
