using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.ReceiptItem.Delete;

public class DeleteReceiptItemCommandHandler(IReceiptItemService receiptitemService) : IRequestHandler<DeleteReceiptItemCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteReceiptItemCommand request, CancellationToken cancellationToken)
	{
		await receiptitemService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}