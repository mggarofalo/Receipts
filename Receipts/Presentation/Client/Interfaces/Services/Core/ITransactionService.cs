using Shared.ViewModels.Core;

namespace Client.Interfaces.Services.Core;

public interface ITransactionService
{
	Task<List<TransactionVM>?> CreateTransactionsAsync(List<TransactionVM> models, CancellationToken cancellationToken = default);
	Task<TransactionVM?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<TransactionVM>?> GetAllTransactionsAsync(CancellationToken cancellationToken = default);
	Task<List<TransactionVM>?> GetTransactionsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default);
	Task<bool> UpdateTransactionsAsync(List<TransactionVM> models, CancellationToken cancellationToken = default);
	Task<bool> DeleteTransactionsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
}
