using Application.Interfaces;
using Application.Models.NormalizedDescriptions;

namespace Application.Queries.NormalizedDescription.PreviewThresholdImpact;

public record PreviewThresholdImpactQuery(
	double AutoAcceptThreshold,
	double PendingReviewThreshold) : IQuery<ThresholdImpactPreview>;
