using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Receipt;

public record CreateReceiptCommand(List<Domain.Core.Receipt> Receipts) : ICommand<List<Domain.Core.Receipt>>;

public class CreateReceiptCommandHandler(IReceiptRepository receiptRepository) : IRequestHandler<CreateReceiptCommand, List<Domain.Core.Receipt>>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<List<Domain.Core.Receipt>> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Receipt> createdEntities = await _receiptRepository.CreateAsync(request.Receipts, cancellationToken);
		await _receiptRepository.SaveChangesAsync(cancellationToken);
		return createdEntities;
	}
}