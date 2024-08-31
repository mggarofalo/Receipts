using Application.Interfaces;

namespace Application.Commands.Receipt;

public record UpdateReceiptCommand(List<Domain.Core.Receipt> Receipts) : ICommand<bool>;
