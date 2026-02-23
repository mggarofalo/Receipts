using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize]
public class ReceiptsHub : Hub<IReceiptsHubClient>
{
	public override async Task OnConnectedAsync()
	{
		string? userId = Context.UserIdentifier;
		if (userId != null)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, userId);
		}

		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		string? userId = Context.UserIdentifier;
		if (userId != null)
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
		}

		await base.OnDisconnectedAsync(exception);
	}
}
