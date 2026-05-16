using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.ReceiptItem.Restore;

public class RestoreReceiptItemCommandHandler(IReceiptItemService receiptItemService) : IRequestHandler<RestoreReceiptItemCommand, bool>
{
	public async ValueTask<bool> Handle(RestoreReceiptItemCommand request, CancellationToken cancellationToken)
	{
		return await receiptItemService.RestoreAsync(request.Id, cancellationToken);
	}
}
