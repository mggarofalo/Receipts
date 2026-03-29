using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Aggregates.Dashboard;

public class GetEarliestReceiptYearQueryHandler(IDashboardService dashboardService)
	: IRequestHandler<GetEarliestReceiptYearQuery, int>
{
	public async Task<int> Handle(GetEarliestReceiptYearQuery request, CancellationToken cancellationToken)
	{
		return await dashboardService.GetEarliestReceiptYearAsync(cancellationToken);
	}
}
