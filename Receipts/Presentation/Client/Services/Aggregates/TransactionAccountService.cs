using Client.Interfaces.Services.Aggregates;
using Shared.HttpClientApiExtensions.Aggregates;
using Shared.ViewModels.Aggregates;

namespace Client.Services.Aggregates;

public class TransactionAccountService(HttpClient httpClient) : ITransactionAccountService
{
	public async Task<TransactionAccountVM?> GetTransactionAccountByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetTransactionAccountByTransactionIdAsync(transactionId, cancellationToken);
	}

	public async Task<List<TransactionAccountVM>?> GetTransactionAccountsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetTransactionAccountsByReceiptIdAsync(receiptId, cancellationToken);
	}
}
