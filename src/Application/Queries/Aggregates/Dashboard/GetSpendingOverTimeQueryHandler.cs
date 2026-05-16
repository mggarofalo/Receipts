using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Mediator;

namespace Application.Queries.Aggregates.Dashboard;

public class GetSpendingOverTimeQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetSpendingOverTimeQuery, SpendingOverTimeResult>
{
	public async ValueTask<SpendingOverTimeResult> Handle(GetSpendingOverTimeQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetSpendingOverTimeAsync(request.StartDate, request.EndDate, request.Granularity, cancellationToken);
	}
}
