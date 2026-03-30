using Application.Interfaces.Services;
using Application.Models.Reports;
using MediatR;

namespace Application.Queries.Aggregates.Reports;

public class GetDuplicateDetectionReportQueryHandler(IReportService reportService)
	: IRequestHandler<GetDuplicateDetectionReportQuery, DuplicateDetectionResult>
{
	public async Task<DuplicateDetectionResult> Handle(GetDuplicateDetectionReportQuery request, CancellationToken cancellationToken)
	{
		return await reportService.GetDuplicatesAsync(
			request.MatchOn,
			request.LocationTolerance,
			request.TotalTolerance,
			cancellationToken);
	}
}
