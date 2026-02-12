using Application.Interfaces;

namespace Application.Queries.Aggregates.Trips;

public record GetTripByReceiptIdQuery : IQuery<Domain.Aggregates.Trip?>
{
	public Guid ReceiptId { get; }
	public const string ReceiptIdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetTripByReceiptIdQuery(Guid receiptId)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmptyExceptionMessage, nameof(receiptId));
		}

		ReceiptId = receiptId;
	}
}