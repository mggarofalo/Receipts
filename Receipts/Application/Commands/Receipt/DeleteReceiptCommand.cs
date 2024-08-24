using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Commands.Receipt;

public record DeleteReceiptCommand(Guid Id) : ICommand<bool>;

public class DeleteReceiptCommandHandler(IReceiptRepository receiptRepository) : IRequestHandler<DeleteReceiptCommand, bool>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<bool> Handle(DeleteReceiptCommand request, CancellationToken cancellationToken)
	{
		bool success = await _receiptRepository.DeleteAsync(request.Id, cancellationToken);

		if (success)
		{
			await _receiptRepository.SaveChangesAsync(cancellationToken);
			return true;
		}

		return false;
	}
}
