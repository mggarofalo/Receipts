using Application.Interfaces;

namespace Application.Commands.Account;

public record DeleteAccountCommand(List<Guid> Ids) : ICommand<bool>;
