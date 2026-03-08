using Application.Interfaces.Services;
using Application.Models.Dashboard;
using MediatR;

namespace Application.Queries.Aggregates.Dashboard;

public class GetSpendingByCategoryQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetSpendingByCategoryQuery, SpendingByCategoryResult>
{
	public async Task<SpendingByCategoryResult> Handle(GetSpendingByCategoryQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetSpendingByCategoryAsync(request.StartDate, request.EndDate, request.Limit, cancellationToken);
	}
}
