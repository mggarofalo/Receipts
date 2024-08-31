using Application.Interfaces;

namespace Application.Commands.ReceiptItem;

public record UpdateReceiptItemCommand(List<Domain.Core.ReceiptItem> ReceiptItems) : ICommand<bool>;
