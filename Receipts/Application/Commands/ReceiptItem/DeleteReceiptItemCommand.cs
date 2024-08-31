using Application.Common;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.ReceiptItem;

public record DeleteReceiptItemCommand(List<Guid> Ids) : ICommand<bool>;

public class DeleteReceiptItemCommandHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<DeleteReceiptItemCommand, bool>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<bool> Handle(DeleteReceiptItemCommand request, CancellationToken cancellationToken)
	{
		bool success = await _receiptitemRepository.DeleteAsync(request.Ids, cancellationToken);

		if (success)
		{
			await _receiptitemRepository.SaveChangesAsync(cancellationToken);
		}

		return success;
	}
}