using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Receipt.Restore;

public class RestoreReceiptCommandHandler(IReceiptService receiptService) : IRequestHandler<RestoreReceiptCommand, bool>
{
	public async ValueTask<bool> Handle(RestoreReceiptCommand request, CancellationToken cancellationToken)
	{
		return await receiptService.RestoreAsync(request.Id, cancellationToken);
	}
}
