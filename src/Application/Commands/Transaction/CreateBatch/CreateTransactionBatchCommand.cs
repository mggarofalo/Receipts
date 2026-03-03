using Application.Interfaces;

namespace Application.Commands.Transaction.CreateBatch;

public record CreateTransactionBatchCommand : ICommand<List<Domain.Core.Transaction>>
{
	public IReadOnlyList<Domain.Core.Transaction> Transactions { get; }
	public Guid ReceiptId { get; }

	public const string TransactionsCannotBeEmptyExceptionMessage = "Transactions list cannot be empty.";

	public CreateTransactionBatchCommand(List<Domain.Core.Transaction> transactions, Guid receiptId)
	{
		ArgumentNullException.ThrowIfNull(transactions);

		if (transactions.Count == 0)
		{
			throw new ArgumentException(TransactionsCannotBeEmptyExceptionMessage, nameof(transactions));
		}

		Transactions = transactions.AsReadOnly();
		ReceiptId = receiptId;
	}
}
