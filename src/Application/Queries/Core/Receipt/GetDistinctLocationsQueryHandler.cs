using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Receipt;

public class GetDistinctLocationsQueryHandler(IReceiptService receiptService) : IRequestHandler<GetDistinctLocationsQuery, List<string>>
{
	public async Task<List<string>> Handle(GetDistinctLocationsQuery request, CancellationToken cancellationToken)
	{
		return await receiptService.GetDistinctLocationsAsync(request.Query, request.Limit, cancellationToken);
	}
}
