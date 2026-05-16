using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Receipt;

public class GetDistinctLocationsQueryHandler(IReceiptService receiptService) : IRequestHandler<GetDistinctLocationsQuery, List<string>>
{
	public async ValueTask<List<string>> Handle(GetDistinctLocationsQuery request, CancellationToken cancellationToken)
	{
		return await receiptService.GetDistinctLocationsAsync(request.Query, request.Limit, cancellationToken);
	}
}
