using Application.Interfaces;

namespace Application.Commands.Transaction.Create;

public record CreateTransactionCommand : ICommand<List<Domain.Core.Transaction>>
{
	public IReadOnlyList<Domain.Core.Transaction> Transactions { get; }
	public Guid ReceiptId { get; }
	public Guid AccountId { get; }

	public const string TransactionsCannotBeEmptyExceptionMessage = "Transactions list cannot be empty.";

	public CreateTransactionCommand(List<Domain.Core.Transaction> transactions, Guid receiptId, Guid accountId)
	{
		ArgumentNullException.ThrowIfNull(transactions);

		if (transactions.Count == 0)
		{
			throw new ArgumentException(TransactionsCannotBeEmptyExceptionMessage, nameof(transactions));
		}

		Transactions = transactions.AsReadOnly();
		ReceiptId = receiptId;
		AccountId = accountId;
	}
}
