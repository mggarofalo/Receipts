using Application.Interfaces;

namespace Application.Commands.Ynab.CategoryMapping;

public record UpdateYnabCategoryMappingCommand(
	Guid Id,
	string YnabCategoryId,
	string YnabCategoryName,
	string YnabCategoryGroupName,
	string YnabBudgetId) : ICommand<Mediator.Unit>;
