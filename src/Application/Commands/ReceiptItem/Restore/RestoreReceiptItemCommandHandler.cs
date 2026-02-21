using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ReceiptItem.Restore;

public class RestoreReceiptItemCommandHandler(IReceiptItemService receiptItemService) : IRequestHandler<RestoreReceiptItemCommand, bool>
{
	public async Task<bool> Handle(RestoreReceiptItemCommand request, CancellationToken cancellationToken)
	{
		return await receiptItemService.RestoreAsync(request.Id, cancellationToken);
	}
}
