using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetItemCostOverTimeQueryHandler(IReportService reportService)
	: IRequestHandler<GetItemCostOverTimeQuery, ItemCostOverTimeResult>
{
	public async Task<ItemCostOverTimeResult> Handle(GetItemCostOverTimeQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetItemCostOverTimeAsync(
			request.Description,
			request.Category,
			request.StartDate,
			request.EndDate,
			request.Granularity,
			cancellationToken);
	}
}
