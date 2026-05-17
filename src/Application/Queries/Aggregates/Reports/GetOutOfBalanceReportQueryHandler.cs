using Application.Interfaces.Services;
using Application.Models.Reports;
using Mediator;

namespace Application.Queries.Aggregates.Reports;

public class GetOutOfBalanceReportQueryHandler(IReportService reportService)
	: IRequestHandler<GetOutOfBalanceReportQuery, OutOfBalanceResult>
{
	public async ValueTask<OutOfBalanceResult> Handle(GetOutOfBalanceReportQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetOutOfBalanceAsync(
			request.SortBy,
			request.SortDirection,
			request.Page,
			request.PageSize,
			cancellationToken);
	}
}
