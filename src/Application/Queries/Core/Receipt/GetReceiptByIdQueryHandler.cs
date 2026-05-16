using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Receipt;

public class GetReceiptByIdQueryHandler(IReceiptService receiptService) : IRequestHandler<GetReceiptByIdQuery, Domain.Core.Receipt?>
{
	public async ValueTask<Domain.Core.Receipt?> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
	{
		return await receiptService.GetByIdAsync(request.Id, cancellationToken);
	}
}
