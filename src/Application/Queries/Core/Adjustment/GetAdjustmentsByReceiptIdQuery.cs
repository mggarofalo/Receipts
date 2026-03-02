using Application.Interfaces;

namespace Application.Queries.Core.Adjustment;

public record GetAdjustmentsByReceiptIdQuery : IQuery<List<Domain.Core.Adjustment>?>
{
	public Guid ReceiptId { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetAdjustmentsByReceiptIdQuery(Guid receiptId)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
	}
}
