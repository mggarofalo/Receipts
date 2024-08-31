using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.ReceiptItem;

public record GetReceiptItemByIdQuery(Guid Id) : IQuery<Domain.Core.ReceiptItem?>;

public class GetReceiptItemByIdQueryHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<GetReceiptItemByIdQuery, Domain.Core.ReceiptItem?>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<Domain.Core.ReceiptItem?> Handle(GetReceiptItemByIdQuery request, CancellationToken cancellationToken)
	{
		return await _receiptitemRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
