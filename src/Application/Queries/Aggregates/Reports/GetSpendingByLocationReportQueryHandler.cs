using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetSpendingByLocationReportQueryHandler(IReportService reportService)
	: IRequestHandler<GetSpendingByLocationReportQuery, SpendingByLocationResult>
{
	public async Task<SpendingByLocationResult> Handle(GetSpendingByLocationReportQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetSpendingByLocationAsync(
			request.StartDate,
			request.EndDate,
			request.SortBy,
			request.SortDirection,
			request.Page,
			request.PageSize,
			cancellationToken);
	}
}
