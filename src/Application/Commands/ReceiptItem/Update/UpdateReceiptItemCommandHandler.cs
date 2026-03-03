using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ReceiptItem.Update;

public class UpdateReceiptItemCommandHandler(IReceiptItemService receiptitemService) : IRequestHandler<UpdateReceiptItemCommand, bool>
{
	public async Task<bool> Handle(UpdateReceiptItemCommand request, CancellationToken cancellationToken)
	{
		Domain.Core.ReceiptItem existingItem = await receiptitemService.GetByIdAsync(request.ReceiptItems[0].Id, cancellationToken)
			?? throw new InvalidOperationException("Receipt item not found");

		await receiptitemService.UpdateAsync([.. request.ReceiptItems], existingItem.ReceiptId, cancellationToken);
		return true;
	}
}