using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize]
public class ReceiptsHub : Hub
{
	// Event name constants - used by controllers and frontend
	public const string AccountCreated = "AccountCreated";
	public const string AccountUpdated = "AccountUpdated";
	public const string AccountDeleted = "AccountDeleted";
	public const string ReceiptCreated = "ReceiptCreated";
	public const string ReceiptUpdated = "ReceiptUpdated";
	public const string ReceiptDeleted = "ReceiptDeleted";
	public const string ReceiptItemCreated = "ReceiptItemCreated";
	public const string ReceiptItemUpdated = "ReceiptItemUpdated";
	public const string ReceiptItemDeleted = "ReceiptItemDeleted";
	public const string TransactionCreated = "TransactionCreated";
	public const string TransactionUpdated = "TransactionUpdated";
	public const string TransactionDeleted = "TransactionDeleted";

	public override async Task OnConnectedAsync()
	{
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		await base.OnDisconnectedAsync(exception);
	}
}
