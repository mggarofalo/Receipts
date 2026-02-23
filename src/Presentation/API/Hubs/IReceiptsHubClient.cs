using API.Generated.Dtos;

namespace API.Hubs;

public interface IReceiptsHubClient
{
	Task AccountCreated(AccountResponse account);
	Task AccountUpdated(Guid id);
	Task AccountDeleted(Guid id);
	Task ReceiptCreated(ReceiptResponse receipt);
	Task ReceiptUpdated(Guid id);
	Task ReceiptDeleted(Guid id);
	Task ReceiptItemCreated(ReceiptItemResponse receiptItem);
	Task ReceiptItemUpdated(Guid id);
	Task ReceiptItemDeleted(Guid id);
	Task TransactionCreated(TransactionResponse transaction);
	Task TransactionUpdated(Guid id);
	Task TransactionDeleted(Guid id);
}
