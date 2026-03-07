using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Adjustment;

public record GetAdjustmentsByReceiptIdQuery : IQuery<PagedResult<Domain.Core.Adjustment>>
{
	public Guid ReceiptId { get; }
	public int Offset { get; }
	public int Limit { get; }
	public SortParams Sort { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetAdjustmentsByReceiptIdQuery(Guid receiptId, int offset, int limit, SortParams sort)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
		Offset = offset;
		Limit = limit;
		Sort = sort;
	}
}
