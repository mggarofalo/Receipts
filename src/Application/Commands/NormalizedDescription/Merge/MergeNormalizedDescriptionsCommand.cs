using Application.Interfaces;

namespace Application.Commands.NormalizedDescription.Merge;

// Merges two NormalizedDescription rows. All ReceiptItems currently pointing at DiscardId are
// re-linked to KeepId, then the DiscardId row is deleted. The handler returns the count of
// items that were actually re-linked — 0 if either id was missing or if KeepId == DiscardId,
// which is the service contract.
public record MergeNormalizedDescriptionsCommand(Guid KeepId, Guid DiscardId) : ICommand<int>;
