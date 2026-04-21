using Application.Interfaces.Services;
using Application.Models.NormalizedDescriptions;
using MediatR;

namespace Application.Queries.NormalizedDescription.TestMatch;

public class TestNormalizedDescriptionMatchQueryHandler(INormalizedDescriptionService service)
	: IRequestHandler<TestNormalizedDescriptionMatchQuery, MatchTestResult>
{
	public async Task<MatchTestResult> Handle(TestNormalizedDescriptionMatchQuery request, CancellationToken cancellationToken)
	{
		return await service.TestMatchAsync(
			request.Description,
			request.TopN,
			request.AutoAcceptThresholdOverride,
			request.PendingReviewThresholdOverride,
			cancellationToken);
	}
}
