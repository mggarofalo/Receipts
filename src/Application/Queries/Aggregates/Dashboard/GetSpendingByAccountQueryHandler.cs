using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Mediator;

namespace Application.Queries.Aggregates.Dashboard;

public class GetSpendingByAccountQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetSpendingByAccountQuery, SpendingByAccountResult>
{
	public async ValueTask<SpendingByAccountResult> Handle(GetSpendingByAccountQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetSpendingByAccountAsync(request.StartDate, request.EndDate, cancellationToken);
	}
}
