using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetOutOfBalanceReportQueryHandler(IReportService reportService)
	: IRequestHandler<GetOutOfBalanceReportQuery, OutOfBalanceResult>
{
	public async Task<OutOfBalanceResult> Handle(GetOutOfBalanceReportQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetOutOfBalanceAsync(
			request.SortBy,
			request.SortDirection,
			request.Page,
			request.PageSize,
			cancellationToken);
	}
}
