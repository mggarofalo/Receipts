using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Receipt;

public class CreateReceiptCommandHandler(IReceiptRepository receiptRepository) : IRequestHandler<CreateReceiptCommand, List<Domain.Core.Receipt>>
{
	public async Task<List<Domain.Core.Receipt>> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Receipt> createdEntities = await receiptRepository.CreateAsync([.. request.Receipts], cancellationToken);
		return createdEntities;
	}
}