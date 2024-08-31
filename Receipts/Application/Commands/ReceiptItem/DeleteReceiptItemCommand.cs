using Application.Interfaces;

namespace Application.Commands.ReceiptItem;

public record DeleteReceiptItemCommand(List<Guid> Ids) : ICommand<bool>;
