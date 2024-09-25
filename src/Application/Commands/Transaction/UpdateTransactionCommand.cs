using Application.Interfaces;

namespace Application.Commands.Transaction;

public record UpdateTransactionCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Transaction> Transactions { get; }
	public Guid ReceiptId { get; }
	public Guid AccountId { get; }

	public const string TransactionsCannotBeEmptyExceptionMessage = "Transactions list cannot be empty.";

	public UpdateTransactionCommand(List<Domain.Core.Transaction> transactions, Guid receiptId, Guid accountId)
	{
		ArgumentNullException.ThrowIfNull(transactions);
		ArgumentNullException.ThrowIfNull(receiptId);
		ArgumentNullException.ThrowIfNull(accountId);

		if (transactions.Count == 0)
		{
			throw new ArgumentException(TransactionsCannotBeEmptyExceptionMessage, nameof(transactions));
		}

		Transactions = transactions.AsReadOnly();
		ReceiptId = receiptId;
		AccountId = accountId;
	}
}
