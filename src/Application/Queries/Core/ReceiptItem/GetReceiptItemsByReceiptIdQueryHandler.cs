using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetReceiptItemsByReceiptIdQuery, PagedResult<Domain.Core.ReceiptItem>>
{
	public async Task<PagedResult<Domain.Core.ReceiptItem>> Handle(GetReceiptItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetByReceiptIdAsync(request.ReceiptId, request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
