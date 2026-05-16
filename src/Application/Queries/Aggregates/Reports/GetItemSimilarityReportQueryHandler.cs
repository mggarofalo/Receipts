using Application.Interfaces.Services;
using Application.Models.Reports;
using Mediator;

namespace Application.Queries.Aggregates.Reports;

public class GetItemSimilarityReportQueryHandler(IReportService reportService)
	: IRequestHandler<GetItemSimilarityReportQuery, ItemSimilarityResult>
{
	public async ValueTask<ItemSimilarityResult> Handle(GetItemSimilarityReportQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetItemSimilarityAsync(
			request.Threshold,
			request.SortBy,
			request.SortDirection,
			request.Page,
			request.PageSize,
			cancellationToken);
	}
}
