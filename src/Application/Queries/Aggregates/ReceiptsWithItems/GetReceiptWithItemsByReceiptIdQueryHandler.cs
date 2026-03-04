using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Aggregates.ReceiptsWithItems;

public class GetReceiptWithItemsByReceiptIdQueryHandler(
	IReceiptService receiptService,
	IReceiptItemService receiptItemService,
	IAdjustmentService adjustmentService
) : IRequestHandler<GetReceiptWithItemsByReceiptIdQuery, Domain.Aggregates.ReceiptWithItems?>
{
	public async Task<Domain.Aggregates.ReceiptWithItems?> Handle(GetReceiptWithItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		Domain.Core.Receipt? receipt = await receiptService.GetByIdAsync(request.ReceiptId, cancellationToken);

		if (receipt == null)
		{
			return null;
		}

		var receiptItemsResult = await receiptItemService.GetByReceiptIdAsync(request.ReceiptId, 0, int.MaxValue, cancellationToken);
		var adjustmentsResult = await adjustmentService.GetByReceiptIdAsync(request.ReceiptId, 0, int.MaxValue, cancellationToken);

		return new Domain.Aggregates.ReceiptWithItems()
		{
			Receipt = receipt,
			Items = receiptItemsResult.Data,
			Adjustments = adjustmentsResult.Data
		};
	}
}
