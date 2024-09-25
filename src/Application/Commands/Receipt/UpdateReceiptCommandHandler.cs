using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Receipt;

public class UpdateReceiptCommandHandler(IReceiptRepository receiptRepository) : IRequestHandler<UpdateReceiptCommand, bool>
{
	public async Task<bool> Handle(UpdateReceiptCommand request, CancellationToken cancellationToken)
	{
		await receiptRepository.UpdateAsync([.. request.Receipts], cancellationToken);
		return true;
	}
}