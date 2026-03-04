namespace API.Hubs;

public interface IEntityHubClient
{
	Task EntityChanged(EntityChangeNotification notification);
}
