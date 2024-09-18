using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Services;

public class SignalRService : IAsyncDisposable
{
	private readonly HubConnection _hubConnection;

	public SignalRService(IConfiguration configuration)
	{
		string baseUrl = configuration["API_BASE_URL"] ?? "http://localhost:5136";

		_hubConnection = new HubConnectionBuilder()
			.WithUrl($"{baseUrl}/receipts")
			.WithAutomaticReconnect()
			.Build();
	}

	public async Task StartConnectionAsync()
	{
		await _hubConnection.StartAsync();
	}

	public void RegisterReceiveMessageHandler(Action<string, string> handler)
	{
		_hubConnection.On("ReceiveMessage", handler);
	}

	public async Task SendMessageAsync(string user, string message)
	{
		await _hubConnection.InvokeAsync("SendMessage", user, message);
	}

	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		await _hubConnection.DisposeAsync();
	}
}
