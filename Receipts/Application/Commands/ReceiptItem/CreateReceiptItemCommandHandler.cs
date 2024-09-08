using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.ReceiptItem;

public class CreateReceiptItemCommandHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<CreateReceiptItemCommand, List<Domain.Core.ReceiptItem>>
{
	public async Task<List<Domain.Core.ReceiptItem>> Handle(CreateReceiptItemCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.ReceiptItem> createdEntities = await receiptitemRepository.CreateAsync([.. request.ReceiptItems], request.ReceiptId, cancellationToken);
		await receiptitemRepository.SaveChangesAsync(cancellationToken);
		return createdEntities;
	}
}