using Domain.NormalizedDescriptions;

namespace Application.Interfaces.Services;

public interface INormalizedDescriptionService
{
	Task<NormalizedDescription> GetOrCreateAsync(string rawDescription, CancellationToken cancellationToken);
	Task<NormalizedDescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<NormalizedDescription>> GetAllAsync(NormalizedDescriptionStatus? filter, CancellationToken cancellationToken);
	Task<int> MergeAsync(Guid keepId, Guid discardId, CancellationToken cancellationToken);
	Task<NormalizedDescription> SplitAsync(Guid receiptItemId, CancellationToken cancellationToken);
	Task<bool> UpdateStatusAsync(Guid id, NormalizedDescriptionStatus status, CancellationToken cancellationToken);
}
