using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Ynab.SelectBudget;

public class SelectYnabBudgetCommandHandler(IYnabBudgetSelectionService budgetSelectionService) : IRequestHandler<SelectYnabBudgetCommand, Unit>
{
	public async ValueTask<Unit> Handle(SelectYnabBudgetCommand request, CancellationToken cancellationToken)
	{
		await budgetSelectionService.SetSelectedBudgetIdAsync(request.BudgetId, cancellationToken);
		return Unit.Value;
	}
}
