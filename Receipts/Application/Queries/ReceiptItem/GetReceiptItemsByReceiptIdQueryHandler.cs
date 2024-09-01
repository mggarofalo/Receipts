using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<GetReceiptItemsByReceiptIdQuery, List<Domain.Core.ReceiptItem>>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<List<Domain.Core.ReceiptItem>> Handle(GetReceiptItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		// TODO: Update repo to return null if receiptId doesn't exist and empty if receiptId exists but has no items
		return await _receiptitemRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
