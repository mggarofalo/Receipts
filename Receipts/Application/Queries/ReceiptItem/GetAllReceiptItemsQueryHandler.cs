using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.ReceiptItem;

public class GetAllReceiptItemsQueryHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<GetAllReceiptItemsQuery, List<Domain.Core.ReceiptItem>>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<List<Domain.Core.ReceiptItem>> Handle(GetAllReceiptItemsQuery request, CancellationToken cancellationToken)
	{
		return await _receiptitemRepository.GetAllAsync(cancellationToken);
	}
}
