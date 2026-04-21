using Application.Interfaces;
using Application.Models.NormalizedDescriptions;

namespace Application.Queries.NormalizedDescription.TestMatch;

public record TestNormalizedDescriptionMatchQuery(
	string Description,
	int TopN,
	double? AutoAcceptThresholdOverride,
	double? PendingReviewThresholdOverride) : IQuery<MatchTestResult>;
