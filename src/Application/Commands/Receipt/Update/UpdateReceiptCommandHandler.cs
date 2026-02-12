using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.Update;

public class UpdateReceiptCommandHandler(IReceiptService receiptService) : IRequestHandler<UpdateReceiptCommand, bool>
{
	public async Task<bool> Handle(UpdateReceiptCommand request, CancellationToken cancellationToken)
	{
		await receiptService.UpdateAsync([.. request.Receipts], cancellationToken);
		return true;
	}
}