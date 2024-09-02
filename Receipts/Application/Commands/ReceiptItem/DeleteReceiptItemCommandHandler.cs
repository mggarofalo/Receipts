using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.ReceiptItem;

public class DeleteReceiptItemCommandHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<DeleteReceiptItemCommand, bool>
{
	public async Task<bool> Handle(DeleteReceiptItemCommand request, CancellationToken cancellationToken)
	{
		await receiptitemRepository.DeleteAsync([.. request.Ids], cancellationToken);
		await receiptitemRepository.SaveChangesAsync(cancellationToken);
		return true;
	}
}