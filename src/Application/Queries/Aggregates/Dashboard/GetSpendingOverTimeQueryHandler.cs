using Application.Interfaces.Services;
using Application.Models.Dashboard;
using MediatR;

namespace Application.Queries.Aggregates.Dashboard;

public class GetSpendingOverTimeQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetSpendingOverTimeQuery, SpendingOverTimeResult>
{
	public async Task<SpendingOverTimeResult> Handle(GetSpendingOverTimeQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetSpendingOverTimeAsync(request.StartDate, request.EndDate, request.Granularity, cancellationToken);
	}
}
