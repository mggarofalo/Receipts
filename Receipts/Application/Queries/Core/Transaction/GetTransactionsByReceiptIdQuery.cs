using Application.Interfaces;

namespace Application.Queries.Core.Transaction;

public record GetTransactionsByReceiptIdQuery : IQuery<List<Domain.Core.Transaction>>
{
	public Guid ReceiptId { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetTransactionsByReceiptIdQuery(Guid receiptId)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
	}
}
