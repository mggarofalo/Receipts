using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Core.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<GetReceiptItemsByReceiptIdQuery, List<Domain.Core.ReceiptItem>?>
{
	public async Task<List<Domain.Core.ReceiptItem>?> Handle(GetReceiptItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
