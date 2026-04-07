using Application.Interfaces.Services;
using Application.Models.Ynab;
using Infrastructure.Ynab;

namespace Infrastructure.Services;

public class YnabRateLimitTracker : IYnabRateLimitTracker
{
	private readonly Queue<DateTimeOffset> _requestTimestamps = new();
	private readonly object _lock = new();
	private readonly YnabClientOptions _options;
	private readonly TimeProvider _timeProvider;

	public YnabRateLimitTracker(YnabClientOptions options, TimeProvider timeProvider)
	{
		_options = options;
		_timeProvider = timeProvider;
	}

	public void RecordRequest()
	{
		lock (_lock)
		{
			PruneExpired();
			_requestTimestamps.Enqueue(_timeProvider.GetUtcNow());
		}
	}

	public YnabRateLimitStatus GetStatus()
	{
		lock (_lock)
		{
			PruneExpired();

			int requestsUsed = _requestTimestamps.Count;
			int remaining = Math.Max(0, _options.RateLimitMaxRequests - requestsUsed);

			DateTimeOffset? windowResetAt = null;
			DateTimeOffset? oldestRequestAt = null;

			if (_requestTimestamps.TryPeek(out DateTimeOffset oldest))
			{
				oldestRequestAt = oldest;
				windowResetAt = oldest.Add(TimeSpan.FromSeconds(_options.RateLimitWindowSeconds));
			}

			return new YnabRateLimitStatus(
				remaining,
				_options.RateLimitMaxRequests,
				requestsUsed,
				windowResetAt,
				oldestRequestAt);
		}
	}

	public bool CanMakeRequests(int count)
	{
		lock (_lock)
		{
			PruneExpired();
			int requestsUsed = _requestTimestamps.Count;
			return requestsUsed + count <= _options.RateLimitMaxRequests;
		}
	}

	private void PruneExpired()
	{
		DateTimeOffset cutoff = _timeProvider.GetUtcNow().AddSeconds(-_options.RateLimitWindowSeconds);

		while (_requestTimestamps.TryPeek(out DateTimeOffset oldest) && oldest < cutoff)
		{
			_requestTimestamps.Dequeue();
		}
	}
}
