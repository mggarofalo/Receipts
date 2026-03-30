using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetUncategorizedItemsReportQueryHandler(IReportService reportService)
	: IRequestHandler<GetUncategorizedItemsReportQuery, UncategorizedItemsResult>
{
	public async Task<UncategorizedItemsResult> Handle(GetUncategorizedItemsReportQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetUncategorizedItemsAsync(
			request.SortBy,
			request.SortDirection,
			request.Page,
			request.PageSize,
			cancellationToken);
	}
}
