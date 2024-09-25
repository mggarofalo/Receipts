using Shared.ViewModels.Aggregates;

namespace Client.Interfaces.Services.Aggregates;

public interface ITransactionAccountService
{
	Task<TransactionAccountVM?> GetTransactionAccountByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
	Task<List<TransactionAccountVM>?> GetTransactionAccountsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default);
}
