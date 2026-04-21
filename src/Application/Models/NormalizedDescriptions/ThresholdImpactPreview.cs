namespace Application.Models.NormalizedDescriptions;

// Result of INormalizedDescriptionService.PreviewThresholdImpactAsync. Counts are computed
// against the live ReceiptItems.NormalizedDescriptionMatchScore column, once with the current
// DB thresholds and once with the proposed ones. The deltas are an admin-friendly breakdown
// of how many items would change classification bucket. Items with a NULL score contribute
// to the Unresolved bucket in both sides.
public record ThresholdImpactPreview(
	ClassificationCounts Current,
	ClassificationCounts Proposed,
	ReclassificationDeltas Deltas);

public record ClassificationCounts(int AutoAccepted, int PendingReview, int Unresolved);

public record ReclassificationDeltas(
	int AutoToPending,
	int PendingToAuto,
	int UnresolvedToAuto,
	int UnresolvedToPending);
