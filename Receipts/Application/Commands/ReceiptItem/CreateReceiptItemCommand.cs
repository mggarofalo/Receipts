using Application.Interfaces;

namespace Application.Commands.ReceiptItem;

public record CreateReceiptItemCommand(List<Domain.Core.ReceiptItem> ReceiptItems) : ICommand<List<Domain.Core.ReceiptItem>>;
