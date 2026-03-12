using System.Security.Claims;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class EntityHub(ISignalRConnectionTracker connectionTracker) : Hub<IEntityHubClient>
{
	public override Task OnConnectedAsync()
	{
		var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is not null)
		{
			connectionTracker.TrackConnection(Context.ConnectionId, userId);
		}
		return base.OnConnectedAsync();
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		connectionTracker.RemoveConnection(Context.ConnectionId);
		return base.OnDisconnectedAsync(exception);
	}
}
