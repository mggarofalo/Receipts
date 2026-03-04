using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Transaction;

public record GetTransactionsByReceiptIdQuery : IQuery<PagedResult<Domain.Core.Transaction>>
{
	public Guid ReceiptId { get; }
	public int Offset { get; }
	public int Limit { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetTransactionsByReceiptIdQuery(Guid receiptId, int offset, int limit)
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
