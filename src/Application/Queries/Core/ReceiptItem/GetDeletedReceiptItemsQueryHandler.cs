using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.ReceiptItem;

public class GetDeletedReceiptItemsQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetDeletedReceiptItemsQuery, PagedResult<Domain.Core.ReceiptItem>>
{
	public async ValueTask<PagedResult<Domain.Core.ReceiptItem>> Handle(GetDeletedReceiptItemsQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
