using Application.Interfaces;

namespace Application.Queries.Aggregates.ReceiptsWithItems;

public record GetReceiptWithItemsByReceiptIdQuery : IQuery<Domain.Aggregates.ReceiptWithItems?>
{
	public Guid ReceiptId { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetReceiptWithItemsByReceiptIdQuery(Guid receiptId)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
	}
}
