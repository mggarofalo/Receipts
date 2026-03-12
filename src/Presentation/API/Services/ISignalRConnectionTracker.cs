namespace API.Services;

public interface ISignalRConnectionTracker
{
	void TrackConnection(string connectionId, string userId);
	void RemoveConnection(string connectionId);
	bool IsConnectionOwnedBy(string connectionId, string userId);
}
