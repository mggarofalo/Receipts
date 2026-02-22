using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

// Authorization is enforced at the endpoint level via .RequireAuthorization() in Program.cs.
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
}
