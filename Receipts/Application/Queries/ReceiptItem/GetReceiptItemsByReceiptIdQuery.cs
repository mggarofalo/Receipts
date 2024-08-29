using Application.Interfaces;
using MediatR;

namespace Application.Queries.ReceiptItem;

public record GetReceiptItemsByReceiptIdQuery(Guid ReceiptId) : IQuery<List<Domain.Core.ReceiptItem>>;

public class GetReceiptItemByReceiptIdQueryHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<GetReceiptItemsByReceiptIdQuery, List<Domain.Core.ReceiptItem>>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<List<Domain.Core.ReceiptItem>> Handle(GetReceiptItemsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await _receiptitemRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}