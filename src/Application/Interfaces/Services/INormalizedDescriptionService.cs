using Application.Models.NormalizedDescriptions;
using Domain.NormalizedDescriptions;

namespace Application.Interfaces.Services;

public interface INormalizedDescriptionService
{
	Task<GetOrCreateResult> GetOrCreateAsync(string rawDescription, CancellationToken cancellationToken);
	Task<NormalizedDescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<NormalizedDescription>> GetAllAsync(NormalizedDescriptionStatus? filter, CancellationToken cancellationToken);
	Task<int> MergeAsync(Guid keepId, Guid discardId, CancellationToken cancellationToken);
	Task<NormalizedDescription> SplitAsync(Guid receiptItemId, CancellationToken cancellationToken);
	Task<bool> UpdateStatusAsync(Guid id, NormalizedDescriptionStatus status, CancellationToken cancellationToken);

	Task<NormalizedDescriptionSettings> GetSettingsAsync(CancellationToken cancellationToken);
	Task<NormalizedDescriptionSettings> UpdateSettingsAsync(double autoAcceptThreshold, double pendingReviewThreshold, CancellationToken cancellationToken);
	Task<MatchTestResult> TestMatchAsync(string description, int topN, double? autoAcceptThresholdOverride, double? pendingReviewThresholdOverride, CancellationToken cancellationToken);
	Task<ThresholdImpactPreview> PreviewThresholdImpactAsync(double autoAcceptThreshold, double pendingReviewThreshold, CancellationToken cancellationToken);
}
