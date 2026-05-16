using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetReceiptItemsByReceiptIdQuery, PagedResult<Domain.Core.ReceiptItem>>
{
	public async ValueTask<PagedResult<Domain.Core.ReceiptItem>> Handle(GetReceiptItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetByReceiptIdAsync(request.ReceiptId, request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
