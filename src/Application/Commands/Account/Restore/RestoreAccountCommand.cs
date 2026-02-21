using Application.Interfaces;

namespace Application.Commands.Account.Restore;

public record RestoreAccountCommand(Guid Id) : ICommand<bool>;
