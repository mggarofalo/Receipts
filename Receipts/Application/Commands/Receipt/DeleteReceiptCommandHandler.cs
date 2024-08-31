using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Receipt;

public class DeleteReceiptCommandHandler(IReceiptRepository receiptRepository) : IRequestHandler<DeleteReceiptCommand, bool>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<bool> Handle(DeleteReceiptCommand request, CancellationToken cancellationToken)
	{
		bool success = await _receiptRepository.DeleteAsync(request.Ids, cancellationToken);

		if (success)
		{
			await _receiptRepository.SaveChangesAsync(cancellationToken);
		}

		return success;
	}
}