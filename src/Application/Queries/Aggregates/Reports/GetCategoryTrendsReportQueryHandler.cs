using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetCategoryTrendsReportQueryHandler(IReportService reportService)
	: IRequestHandler<GetCategoryTrendsReportQuery, CategoryTrendsResult>
{
	public async Task<CategoryTrendsResult> Handle(GetCategoryTrendsReportQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetCategoryTrendsAsync(
			request.StartDate,
			request.EndDate,
			request.Granularity,
			request.TopN,
			cancellationToken);
	}
}
