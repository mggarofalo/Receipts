using Application.Interfaces;
using Domain.NormalizedDescriptions;

namespace Application.Commands.NormalizedDescription.UpdateSettings;

public record UpdateNormalizedDescriptionSettingsCommand(
	double AutoAcceptThreshold,
	double PendingReviewThreshold) : ICommand<NormalizedDescriptionSettings>;
