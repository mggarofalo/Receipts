using Application.Interfaces.Services;
using Application.Models.Dashboard;
using MediatR;

namespace Application.Queries.Aggregates.Dashboard;

public class GetSpendingByStoreQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetSpendingByStoreQuery, SpendingByStoreResult>
{
	public async Task<SpendingByStoreResult> Handle(GetSpendingByStoreQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetSpendingByStoreAsync(request.StartDate, request.EndDate, cancellationToken);
	}
}
