using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Commands.ReceiptItem;

public record CreateReceiptItemCommand(List<Domain.Core.ReceiptItem> ReceiptItems) : ICommand<List<Domain.Core.ReceiptItem>>;

public class CreateReceiptItemCommandHandler(IReceiptItemRepository receiptitemRepository) : IRequestHandler<CreateReceiptItemCommand, List<Domain.Core.ReceiptItem>>
{
	private readonly IReceiptItemRepository _receiptitemRepository = receiptitemRepository;

	public async Task<List<Domain.Core.ReceiptItem>> Handle(CreateReceiptItemCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.ReceiptItem> createdEntities = await _receiptitemRepository.CreateAsync(request.ReceiptItems, cancellationToken);
		await _receiptitemRepository.SaveChangesAsync(cancellationToken);
		return createdEntities;
	}
}