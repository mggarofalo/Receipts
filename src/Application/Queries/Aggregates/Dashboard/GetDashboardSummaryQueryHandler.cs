using Application.Interfaces.Services;
using Application.Models.Dashboard;
using MediatR;

namespace Application.Queries.Aggregates.Dashboard;

public class GetDashboardSummaryQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResult>
{
	public async Task<DashboardSummaryResult> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetSummaryAsync(request.StartDate, request.EndDate, cancellationToken);
	}
}
