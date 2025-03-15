using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.Delete;

public class DeleteReceiptCommandHandler(IReceiptService receiptService) : IRequestHandler<DeleteReceiptCommand, bool>
{
	public async Task<bool> Handle(DeleteReceiptCommand request, CancellationToken cancellationToken)
	{
		await receiptService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}