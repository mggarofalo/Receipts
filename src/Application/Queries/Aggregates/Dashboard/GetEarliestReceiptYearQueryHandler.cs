using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Aggregates.Dashboard;

public class GetEarliestReceiptYearQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetEarliestReceiptYearQuery, int>
{
	public async ValueTask<int> Handle(GetEarliestReceiptYearQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetEarliestReceiptYearAsync(cancellationToken);
	}
}
