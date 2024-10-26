using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ReceiptItem;

public class UpdateReceiptItemCommandHandler(IReceiptItemService receiptitemService) : IRequestHandler<UpdateReceiptItemCommand, bool>
{
	public async Task<bool> Handle(UpdateReceiptItemCommand request, CancellationToken cancellationToken)
	{
		await receiptitemService.UpdateAsync([.. request.ReceiptItems], request.ReceiptId, cancellationToken);
		return true;
	}
}