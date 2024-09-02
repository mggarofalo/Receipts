using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.ReceiptItem;

public class UpdateReceiptItemCommandHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<UpdateReceiptItemCommand, bool>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<bool> Handle(UpdateReceiptItemCommand request, CancellationToken cancellationToken)
	{
		await _receiptitemRepository.UpdateAsync([.. request.ReceiptItems], cancellationToken);
		await _receiptitemRepository.SaveChangesAsync(cancellationToken);
		return true;
	}
}