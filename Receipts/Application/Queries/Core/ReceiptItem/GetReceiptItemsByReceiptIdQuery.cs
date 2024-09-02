using Application.Interfaces;

namespace Application.Queries.Core.ReceiptItem;

public record GetReceiptItemsByReceiptIdQuery : IQuery<List<Domain.Core.ReceiptItem>>
{
	public Guid ReceiptId { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetReceiptItemsByReceiptIdQuery(Guid receiptId)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
	}
}
