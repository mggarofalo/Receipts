using Application.Interfaces.Services;
using Application.Models.NormalizedDescriptions;
using MediatR;

namespace Application.Queries.NormalizedDescription.PreviewThresholdImpact;

public class PreviewThresholdImpactQueryHandler(INormalizedDescriptionService service)
	: IRequestHandler<PreviewThresholdImpactQuery, ThresholdImpactPreview>
{
	public async Task<ThresholdImpactPreview> Handle(PreviewThresholdImpactQuery request, CancellationToken cancellationToken)
	{
		return await service.PreviewThresholdImpactAsync(
			request.AutoAcceptThreshold,
			request.PendingReviewThreshold,
			cancellationToken);
	}
}
