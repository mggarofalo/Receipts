// Potential issues with this class:

// 1. Error Handling: There's no error handling for connection failures or message sending failures.
//    Consider adding try-catch blocks and implementing a retry mechanism.

// 2. Connection State: The class doesn't track the connection state. It might try to send messages
//    when the connection is not established.

// 3. Dependency Injection: The class is using IConfiguration directly. Consider injecting an
//    IOptions<T> instead for better testability and configuration management.

// 4. Hardcoded Values: Many string constants are hardcoded. Consider moving these to a configuration file.

// 5. Single Responsibility: The class is handling both connection management and message sending/receiving.
//    Consider splitting these responsibilities.

// 6. Lack of Logging: There's no logging implemented, which could make debugging difficult in production.

// 7. Thread Safety: The class is not thread-safe. If multiple threads access it simultaneously,
//    it could lead to race conditions.

// 8. Disposal Pattern: The DisposeAsync method is implemented, but there's no check if the connection
//    is already disposed, which could lead to potential issues.

// 9. Reconnection Logic: While WithAutomaticReconnect() is used, there's no custom logic to handle
//    what happens during or after a reconnection.

// 10. Lack of Interface: The class is not implementing an interface, which makes it harder to mock
//     in unit tests and less flexible for dependency injection.

using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Services;

public class SignalRService : IAsyncDisposable
{
	private const string BASE_URL = "http://localhost:5136";
	private const string RECEIPTS_ENDPOINT = "/receipts";
	private const string RECEIVE_MESSAGE_METHOD = "ReceiveMessage";
	private const string SEND_MESSAGE_METHOD = "SendMessage";

	private readonly HubConnection _hubConnection;

	public SignalRService()
	{
		_hubConnection = new HubConnectionBuilder()
			.WithUrl($"{BASE_URL}{RECEIPTS_ENDPOINT}")
			.WithAutomaticReconnect()
			.Build();
	}

	public async Task StartConnectionAsync()
	{
		await _hubConnection.StartAsync();
	}

	public void RegisterReceiveMessageHandler(Action<string, string> handler)
	{
		_hubConnection.On(RECEIVE_MESSAGE_METHOD, handler);
	}

	public async Task SendMessageAsync(string user, string message)
	{
		await _hubConnection.InvokeAsync(SEND_MESSAGE_METHOD, user, message);
	}

	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		await _hubConnection.DisposeAsync();
	}
}
