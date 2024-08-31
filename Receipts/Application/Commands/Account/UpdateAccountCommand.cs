using Application.Interfaces;

namespace Application.Commands.Account;

public record UpdateAccountCommand(List<Domain.Core.Account> Accounts) : ICommand<bool>;
