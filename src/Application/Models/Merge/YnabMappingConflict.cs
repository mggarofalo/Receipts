namespace Application.Models.Merge;

public record YnabMappingConflict(
	Guid AccountId,
	string AccountName,
	string YnabBudgetId,
	string YnabAccountId,
	string YnabAccountName);
