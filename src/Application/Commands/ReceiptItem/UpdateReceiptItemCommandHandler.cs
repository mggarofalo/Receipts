using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.ReceiptItem;

public class UpdateReceiptItemCommandHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<UpdateReceiptItemCommand, bool>
{
	public async Task<bool> Handle(UpdateReceiptItemCommand request, CancellationToken cancellationToken)
	{
		await receiptitemRepository.UpdateAsync([.. request.ReceiptItems], request.ReceiptId, cancellationToken);
		return true;
	}
}