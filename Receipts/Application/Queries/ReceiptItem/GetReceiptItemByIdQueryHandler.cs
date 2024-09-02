using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.ReceiptItem;

public class GetReceiptItemByIdQueryHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<GetReceiptItemByIdQuery, Domain.Core.ReceiptItem?>
{
	public async Task<Domain.Core.ReceiptItem?> Handle(GetReceiptItemByIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptitemRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
