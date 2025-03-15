using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ReceiptItem.Delete;

public class DeleteReceiptItemCommandHandler(IReceiptItemService receiptitemService) : IRequestHandler<DeleteReceiptItemCommand, bool>
{
	public async Task<bool> Handle(DeleteReceiptItemCommand request, CancellationToken cancellationToken)
	{
		await receiptitemService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}