using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.ReceiptItem;

public class GetDeletedReceiptItemsQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetDeletedReceiptItemsQuery, PagedResult<Domain.Core.ReceiptItem>>
{
	public async Task<PagedResult<Domain.Core.ReceiptItem>> Handle(GetDeletedReceiptItemsQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
