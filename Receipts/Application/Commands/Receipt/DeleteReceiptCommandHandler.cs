using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Receipt;

public class DeleteReceiptCommandHandler(IReceiptRepository receiptRepository) : IRequestHandler<DeleteReceiptCommand, bool>
{
	public async Task<bool> Handle(DeleteReceiptCommand request, CancellationToken cancellationToken)
	{
		await receiptRepository.DeleteAsync([.. request.Ids], cancellationToken);
		await receiptRepository.SaveChangesAsync(cancellationToken);
		return true;
	}
}