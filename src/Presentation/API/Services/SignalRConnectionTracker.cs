using System.Collections.Concurrent;

namespace API.Services;

public sealed class SignalRConnectionTracker : ISignalRConnectionTracker
{
	private readonly ConcurrentDictionary<string, string> _connectionToUser = new();

	public void TrackConnection(string connectionId, string userId)
	{
		_connectionToUser[connectionId] = userId;
	}

	public void RemoveConnection(string connectionId)
	{
		_connectionToUser.TryRemove(connectionId, out _);
	}

	public bool IsConnectionOwnedBy(string connectionId, string userId)
	{
		return _connectionToUser.TryGetValue(connectionId, out string? owner) && owner == userId;
	}
}
