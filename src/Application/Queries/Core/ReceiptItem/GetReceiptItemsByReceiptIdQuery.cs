using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.ReceiptItem;

public record GetReceiptItemsByReceiptIdQuery : IQuery<PagedResult<Domain.Core.ReceiptItem>>
{
	public Guid ReceiptId { get; }
	public int Offset { get; }
	public int Limit { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetReceiptItemsByReceiptIdQuery(Guid receiptId, int offset, int limit)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
		Offset = offset;
		Limit = limit;
	}
}
