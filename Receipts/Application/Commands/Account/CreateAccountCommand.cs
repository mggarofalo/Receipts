using Application.Interfaces;

namespace Application.Commands.Account;

public record CreateAccountCommand(List<Domain.Core.Account> Accounts) : ICommand<List<Domain.Core.Account>>;
