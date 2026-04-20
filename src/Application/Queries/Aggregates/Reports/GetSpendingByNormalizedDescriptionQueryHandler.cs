using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetSpendingByNormalizedDescriptionQueryHandler(IReportService reportService)
	: IRequestHandler<GetSpendingByNormalizedDescriptionQuery, SpendingByNormalizedDescriptionResult>
{
	public async Task<SpendingByNormalizedDescriptionResult> Handle(GetSpendingByNormalizedDescriptionQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetSpendingByNormalizedDescriptionAsync(
			request.From,
			request.To,
			cancellationToken);
	}
}
