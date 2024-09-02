using Application.Interfaces;

namespace Application.Queries.Aggregates.TransactionAccounts;

public record GetTransactionAccountsByReceiptIdQuery : IQuery<List<Domain.Aggregates.TransactionAccount>>
{
	public Guid ReceiptId { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetTransactionAccountsByReceiptIdQuery(Guid receiptId)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
	}
}