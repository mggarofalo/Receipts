using Application.Interfaces;

namespace Application.Commands.Receipt;

public record CreateReceiptCommand(List<Domain.Core.Receipt> Receipts) : ICommand<List<Domain.Core.Receipt>>;
