namespace Application.Models.Merge;

public record MergeCardsResult(
	bool Success,
	IReadOnlyList<YnabMappingConflict>? Conflicts);
