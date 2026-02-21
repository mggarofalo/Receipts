using Application.Interfaces;

namespace Application.Commands.ReceiptItem.Restore;

public record RestoreReceiptItemCommand(Guid Id) : ICommand<bool>;
