using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ReceiptItem;

public class GetDeletedReceiptItemsQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetDeletedReceiptItemsQuery, List<Domain.Core.ReceiptItem>>
{
	public async Task<List<Domain.Core.ReceiptItem>> Handle(GetDeletedReceiptItemsQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetDeletedAsync(cancellationToken);
	}
}
