using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Aggregates.ReceiptsWithItems;

public class GetReceiptWithItemsByReceiptIdQueryHandler(
	IReceiptService receiptService,
	IReceiptItemService receiptItemService
) : IRequestHandler<GetReceiptWithItemsByReceiptIdQuery, Domain.Aggregates.ReceiptWithItems?>
{
	public async Task<Domain.Aggregates.ReceiptWithItems?> Handle(GetReceiptWithItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		Domain.Core.Receipt? receipt = await receiptService.GetByIdAsync(request.ReceiptId, cancellationToken);

		if (receipt == null)
		{
			return null;
		}

		List<Domain.Core.ReceiptItem>? receiptItems = await receiptItemService.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);

		return new Domain.Aggregates.ReceiptWithItems()
		{
			Receipt = receipt,
			Items = receiptItems!
		};
	}
}
