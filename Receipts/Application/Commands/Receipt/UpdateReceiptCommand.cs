using Application.Common;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Receipt;

public record UpdateReceiptCommand(List<Domain.Core.Receipt> Receipts) : ICommand<bool>;

public class UpdateReceiptCommandHandler(IReceiptRepository receiptRepository) : IRequestHandler<UpdateReceiptCommand, bool>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<bool> Handle(UpdateReceiptCommand request, CancellationToken cancellationToken)
	{
		bool success = await _receiptRepository.UpdateAsync(request.Receipts, cancellationToken);

		if (success)
		{
			await _receiptRepository.SaveChangesAsync(cancellationToken);
		}

		return success;
	}
}