using Application.Interfaces;
using Application.Models.Merge;

namespace Application.Commands.Card.Merge;

public record MergeCardsIntoAccountCommand : ICommand<MergeCardsResult>
{
	public Guid TargetAccountId { get; }
	public IReadOnlyList<Guid> SourceCardIds { get; }
	public Guid? YnabMappingWinnerAccountId { get; }

	public const string TargetIdCannotBeEmpty = "Target account id cannot be empty.";
	public const string SourceCardIdsCannotBeEmpty = "Source card ids cannot be empty.";

	public MergeCardsIntoAccountCommand(Guid targetAccountId, IReadOnlyList<Guid> sourceCardIds, Guid? ynabMappingWinnerAccountId = null)
	{
		if (targetAccountId == Guid.Empty)
		{
			throw new ArgumentException(TargetIdCannotBeEmpty, nameof(targetAccountId));
		}

		if (sourceCardIds is null || sourceCardIds.Count == 0)
		{
			throw new ArgumentException(SourceCardIdsCannotBeEmpty, nameof(sourceCardIds));
		}

		TargetAccountId = targetAccountId;
		SourceCardIds = sourceCardIds;
		YnabMappingWinnerAccountId = ynabMappingWinnerAccountId;
	}
}
