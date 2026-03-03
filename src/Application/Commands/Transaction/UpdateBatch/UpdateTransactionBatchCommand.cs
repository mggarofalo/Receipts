using Application.Interfaces;

namespace Application.Commands.Transaction.UpdateBatch;

public record UpdateTransactionBatchCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Transaction> Transactions { get; }

	public const string TransactionsCannotBeEmptyExceptionMessage = "Transactions list cannot be empty.";

	public UpdateTransactionBatchCommand(List<Domain.Core.Transaction> transactions)
	{
		ArgumentNullException.ThrowIfNull(transactions);

		if (transactions.Count == 0)
		{
			throw new ArgumentException(TransactionsCannotBeEmptyExceptionMessage, nameof(transactions));
		}

		Transactions = transactions.AsReadOnly();
	}
}
