using Application.Interfaces;

namespace Application.Commands.Receipt.Restore;

public record RestoreReceiptCommand(Guid Id) : ICommand<bool>;
