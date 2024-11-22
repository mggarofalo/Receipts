using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ReceiptItem;

public class GetAllReceiptItemsQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetAllReceiptItemsQuery, List<Domain.Core.ReceiptItem>>
{
	public async Task<List<Domain.Core.ReceiptItem>> Handle(GetAllReceiptItemsQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetAllAsync(cancellationToken);
	}
}
