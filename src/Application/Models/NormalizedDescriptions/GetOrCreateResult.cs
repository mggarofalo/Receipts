using Domain.NormalizedDescriptions;

namespace Application.Models.NormalizedDescriptions;

// Returned by INormalizedDescriptionService.GetOrCreateAsync. The resolver (RECEIPTS-578)
// uses MatchScore to populate ReceiptItem.NormalizedDescriptionMatchScore at the same time
// it writes the NormalizedDescriptionId FK, so admins can later query threshold-impact
// aggregates without recomputing embeddings. MatchScore is null when no ANN candidate was
// above the pending-review floor (a brand-new canonical entry was created) or when the
// embedding service was unavailable.
public record GetOrCreateResult(NormalizedDescription Description, double? MatchScore);
