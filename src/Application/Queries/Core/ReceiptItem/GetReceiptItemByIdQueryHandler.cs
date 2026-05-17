using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.ReceiptItem;

public class GetReceiptItemByIdQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetReceiptItemByIdQuery, Domain.Core.ReceiptItem?>
{
	public async ValueTask<Domain.Core.ReceiptItem?> Handle(GetReceiptItemByIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetByIdAsync(request.Id, cancellationToken);
	}
}
