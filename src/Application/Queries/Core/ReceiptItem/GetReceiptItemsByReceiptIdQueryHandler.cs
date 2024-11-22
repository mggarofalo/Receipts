using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetReceiptItemsByReceiptIdQuery, List<Domain.Core.ReceiptItem>?>
{
	public async Task<List<Domain.Core.ReceiptItem>?> Handle(GetReceiptItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
