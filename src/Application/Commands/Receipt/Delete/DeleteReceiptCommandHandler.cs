using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Receipt.Delete;

public class DeleteReceiptCommandHandler(IReceiptService receiptService) : IRequestHandler<DeleteReceiptCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteReceiptCommand request, CancellationToken cancellationToken)
	{
		await receiptService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}