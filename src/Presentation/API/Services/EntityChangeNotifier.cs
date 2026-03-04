using API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace API.Services;

public class EntityChangeNotifier(IHubContext<EntityHub, IEntityHubClient> hubContext) : IEntityChangeNotifier
{
	public async Task NotifyCreated(string entityType, Guid id)
	{
		await hubContext.Clients.All.EntityChanged(new EntityChangeNotification(entityType, "created", id));
	}

	public async Task NotifyUpdated(string entityType, Guid id)
	{
		await hubContext.Clients.All.EntityChanged(new EntityChangeNotification(entityType, "updated", id));
	}

	public async Task NotifyDeleted(string entityType, Guid id)
	{
		await hubContext.Clients.All.EntityChanged(new EntityChangeNotification(entityType, "deleted", id));
	}

	public async Task NotifyBulkChanged(string entityType, string changeType, IEnumerable<Guid> ids)
	{
		foreach (Guid id in ids)
		{
			await hubContext.Clients.All.EntityChanged(new EntityChangeNotification(entityType, changeType, id));
		}
	}
}
