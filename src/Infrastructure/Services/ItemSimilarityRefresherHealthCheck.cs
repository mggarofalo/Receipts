using System.Globalization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.Services;

public sealed class ItemSimilarityRefresherHealthCheck : IHealthCheck
{
	// Slack on top of MaxIdleInterval before we call the background loop stuck.
	// Covers normal refresh duration + minor jitter without false alarms.
	internal static readonly TimeSpan StalenessSlack = TimeSpan.FromHours(1);

	private readonly ItemSimilarityEdgeRefresher _refresher;
	private readonly TimeProvider _timeProvider;

	public ItemSimilarityRefresherHealthCheck(
		ItemSimilarityEdgeRefresher refresher,
		TimeProvider timeProvider)
	{
		_refresher = refresher;
		_timeProvider = timeProvider;
	}

	public Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		DateTimeOffset? lastSuccess = _refresher.LastSuccessfulRefreshAt;
		int failures = _refresher.ConsecutiveFailures;

		Dictionary<string, object> data = new()
		{
			["lastSuccessfulRefreshAt"] = lastSuccess?.ToString("o", CultureInfo.InvariantCulture) ?? "never",
			["consecutiveFailures"] = failures,
		};

		if (failures >= ItemSimilarityEdgeRefresher.MaxConsecutiveFailures)
		{
			return Task.FromResult(HealthCheckResult.Unhealthy(
				$"Item-similarity refresher recorded {failures} consecutive failures.",
				data: data));
		}

		if (lastSuccess is null)
		{
			return Task.FromResult(HealthCheckResult.Unhealthy(
				"Item-similarity refresher has not completed a successful refresh yet.",
				data: data));
		}

		TimeSpan threshold = ItemSimilarityEdgeRefresher.MaxIdleInterval + StalenessSlack;
		TimeSpan sinceLastSuccess = _timeProvider.GetUtcNow() - lastSuccess.Value;
		if (sinceLastSuccess > threshold)
		{
			return Task.FromResult(HealthCheckResult.Unhealthy(
				string.Format(
					CultureInfo.InvariantCulture,
					"Item-similarity refresher last succeeded {0:F1}h ago (threshold {1:F1}h).",
					sinceLastSuccess.TotalHours,
					threshold.TotalHours),
				data: data));
		}

		return Task.FromResult(HealthCheckResult.Healthy(
			"Item-similarity refresher healthy.",
			data: data));
	}
}
