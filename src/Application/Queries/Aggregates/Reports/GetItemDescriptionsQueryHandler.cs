using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetItemDescriptionsQueryHandler(IReportService reportService)
	: IRequestHandler<GetItemDescriptionsQuery, ItemDescriptionResult>
{
	public async Task<ItemDescriptionResult> Handle(GetItemDescriptionsQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetItemDescriptionsAsync(
			request.Search,
			request.CategoryOnly,
			request.Limit,
			cancellationToken);
	}
}
