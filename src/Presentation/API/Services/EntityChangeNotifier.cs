using System.Collections.Concurrent;
using API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace API.Services;

public sealed class EntityChangeNotifier : IEntityChangeNotifier, IDisposable
{
	private readonly IHubContext<EntityHub, IEntityHubClient> _hubContext;
	private readonly ConcurrentDictionary<(string EntityType, string ChangeType), NotificationBucket> _pending = new();
	private readonly Timer _flushTimer;
	private readonly TimeSpan _flushInterval;
	private int _disposed;

	public EntityChangeNotifier(IHubContext<EntityHub, IEntityHubClient> hubContext)
		: this(hubContext, TimeSpan.FromSeconds(1))
	{
	}

	internal EntityChangeNotifier(IHubContext<EntityHub, IEntityHubClient> hubContext, TimeSpan flushInterval)
	{
		_hubContext = hubContext;
		_flushInterval = flushInterval;
		_flushTimer = new Timer(_ => _ = FlushAsync(), null, _flushInterval, _flushInterval);
	}

	public Task NotifyCreated(string entityType, Guid id)
	{
		Enqueue(entityType, "created", id);
		return Task.CompletedTask;
	}

	public Task NotifyUpdated(string entityType, Guid id)
	{
		Enqueue(entityType, "updated", id);
		return Task.CompletedTask;
	}

	public Task NotifyDeleted(string entityType, Guid id)
	{
		Enqueue(entityType, "deleted", id);
		return Task.CompletedTask;
	}

	public Task NotifyBulkChanged(string entityType, string changeType, IEnumerable<Guid> ids)
	{
		foreach (Guid id in ids)
		{
			Enqueue(entityType, changeType, id);
		}
		return Task.CompletedTask;
	}

	public Task NotifyAllChanged(string entityType, string changeType)
	{
		Enqueue(entityType, changeType, id: null);
		return Task.CompletedTask;
	}

	private void Enqueue(string entityType, string changeType, Guid? id)
	{
		var key = (entityType, changeType);
		_pending.AddOrUpdate(
			key,
			_ => new NotificationBucket(id),
			(_, bucket) =>
			{
				bucket.Add(id);
				return bucket;
			});
	}

	internal async Task FlushAsync()
	{
		// Snapshot and remove all pending buckets atomically per key
		List<(string EntityType, string ChangeType, int Count)> toSend = [];
		foreach (var key in _pending.Keys)
		{
			if (_pending.TryRemove(key, out NotificationBucket? bucket))
			{
				toSend.Add((key.EntityType, key.ChangeType, bucket.Count));
			}
		}

		foreach (var (entityType, changeType, count) in toSend)
		{
			await _hubContext.Clients.All.EntityChanged(
				new EntityChangeNotification(entityType, changeType, null, count));
		}
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref _disposed, 1) == 0)
		{
			_flushTimer.Dispose();
			// Fire one last flush synchronously to drain pending notifications
			FlushAsync().GetAwaiter().GetResult();
		}
	}

	private sealed class NotificationBucket
	{
		private int _count;

		public NotificationBucket(Guid? initialId)
		{
			_ = initialId; // Individual IDs not needed for aggregated notifications
			_count = 1;
		}

		public int Count => _count;

		public void Add(Guid? id)
		{
			_ = id;
			Interlocked.Increment(ref _count);
		}
	}
}
