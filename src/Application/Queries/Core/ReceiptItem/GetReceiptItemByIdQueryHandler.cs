using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ReceiptItem;

public class GetReceiptItemByIdQueryHandler(IReceiptItemService receiptitemService) : IRequestHandler<GetReceiptItemByIdQuery, Domain.Core.ReceiptItem?>
{
	public async Task<Domain.Core.ReceiptItem?> Handle(GetReceiptItemByIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemService.GetByIdAsync(request.Id, cancellationToken);
	}
}
