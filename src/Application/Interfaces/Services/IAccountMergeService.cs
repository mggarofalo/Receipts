using Application.Models.Merge;

namespace Application.Interfaces.Services;

public interface IAccountMergeService
{
	Task<MergeCardsResult> MergeCardsAsync(
		Guid targetAccountId,
		IReadOnlyList<Guid> sourceCardIds,
		Guid? ynabMappingWinnerAccountId,
		CancellationToken cancellationToken);
}
