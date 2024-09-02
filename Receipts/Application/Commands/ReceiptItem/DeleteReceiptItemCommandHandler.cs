using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.ReceiptItem;

public class DeleteReceiptItemCommandHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<DeleteReceiptItemCommand, bool>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<bool> Handle(DeleteReceiptItemCommand request, CancellationToken cancellationToken)
	{
		await _receiptitemRepository.DeleteAsync([.. request.Ids], cancellationToken);
		await _receiptitemRepository.SaveChangesAsync(cancellationToken);
		return true;
	}
}