using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ReceiptItem;

public class CreateReceiptItemCommandHandler(IReceiptItemService receiptitemService) : IRequestHandler<CreateReceiptItemCommand, List<Domain.Core.ReceiptItem>>
{
	public async Task<List<Domain.Core.ReceiptItem>> Handle(CreateReceiptItemCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.ReceiptItem> createdEntities = await receiptitemService.CreateAsync([.. request.ReceiptItems], request.ReceiptId, cancellationToken);
		return createdEntities;
	}
}