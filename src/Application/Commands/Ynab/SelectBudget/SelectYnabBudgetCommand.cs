using Application.Interfaces;

namespace Application.Commands.Ynab.SelectBudget;

public record SelectYnabBudgetCommand(string BudgetId) : ICommand<Mediator.Unit>;
