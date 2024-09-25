using Client.Interfaces.Services.Core;
using Shared.HttpClientApiExtensions.Core;
using Shared.ViewModels.Core;

namespace Client.Services.Core;

public class TransactionService(HttpClient httpClient) : ITransactionService
{
	public async Task<List<TransactionVM>?> CreateTransactionsAsync(List<TransactionVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.CreateTransactionsAsync(models, cancellationToken);
	}

	public async Task<TransactionVM?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetTransactionByIdAsync(id, cancellationToken);
	}

	public async Task<List<TransactionVM>?> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetAllTransactionsAsync(cancellationToken);
	}

	public async Task<List<TransactionVM>?> GetTransactionsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetTransactionsByReceiptIdAsync(receiptId, cancellationToken);
	}

	public async Task<bool> UpdateTransactionsAsync(List<TransactionVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.UpdateTransactionsAsync(models, cancellationToken);
	}

	public async Task<bool> DeleteTransactionsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.DeleteTransactionsAsync(ids, cancellationToken);
	}
}
