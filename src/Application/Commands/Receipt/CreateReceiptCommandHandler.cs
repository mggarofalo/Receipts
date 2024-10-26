using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt;

public class CreateReceiptCommandHandler(IReceiptService receiptService) : IRequestHandler<CreateReceiptCommand, List<Domain.Core.Receipt>>
{
	public async Task<List<Domain.Core.Receipt>> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Receipt> createdEntities = await receiptService.CreateAsync([.. request.Receipts], cancellationToken);
		return createdEntities;
	}
}