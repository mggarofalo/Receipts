using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Receipt;

public class GetDeletedReceiptsQueryHandler(IReceiptService receiptService) : IRequestHandler<GetDeletedReceiptsQuery, List<Domain.Core.Receipt>>
{
	public async Task<List<Domain.Core.Receipt>> Handle(GetDeletedReceiptsQuery request, CancellationToken cancellationToken)
	{
		return await receiptService.GetDeletedAsync(cancellationToken);
	}
}
