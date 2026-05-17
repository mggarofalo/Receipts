using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Mediator;

namespace Application.Queries.Aggregates.Dashboard;

public class GetSpendingByCategoryQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetSpendingByCategoryQuery, SpendingByCategoryResult>
{
	public async ValueTask<SpendingByCategoryResult> Handle(GetSpendingByCategoryQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetSpendingByCategoryAsync(request.StartDate, request.EndDate, request.Limit, cancellationToken);
	}
}
