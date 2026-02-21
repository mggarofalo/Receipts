using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.Restore;

public class RestoreReceiptCommandHandler(IReceiptService receiptService) : IRequestHandler<RestoreReceiptCommand, bool>
{
	public async Task<bool> Handle(RestoreReceiptCommand request, CancellationToken cancellationToken)
	{
		return await receiptService.RestoreAsync(request.Id, cancellationToken);
	}
}
