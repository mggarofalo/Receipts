using Application.Interfaces;
using Domain.NormalizedDescriptions;

namespace Application.Commands.NormalizedDescription.UpdateStatus;

// Flips a NormalizedDescription between Active and PendingReview. Returns true when the status
// was actually changed, false when the row was missing or already had the requested status —
// matches the service contract, and lets the controller distinguish 404 from no-op.
public record UpdateNormalizedDescriptionStatusCommand(Guid Id, NormalizedDescriptionStatus Status) : ICommand<bool>;
