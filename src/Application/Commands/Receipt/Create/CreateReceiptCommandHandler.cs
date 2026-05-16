using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Receipt.Create;

public class CreateReceiptCommandHandler(IReceiptService receiptService) : IRequestHandler<CreateReceiptCommand, List<Domain.Core.Receipt>>
{
	public async ValueTask<List<Domain.Core.Receipt>> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Receipt> createdEntities = await receiptService.CreateAsync([.. request.Receipts], cancellationToken);
		return createdEntities;
	}
}