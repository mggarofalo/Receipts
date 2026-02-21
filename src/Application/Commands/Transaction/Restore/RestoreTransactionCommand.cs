using Application.Interfaces;

namespace Application.Commands.Transaction.Restore;

public record RestoreTransactionCommand(Guid Id) : ICommand<bool>;
