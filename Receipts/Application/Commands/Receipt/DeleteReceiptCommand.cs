using Application.Interfaces;

namespace Application.Commands.Receipt;

public record DeleteReceiptCommand(List<Guid> Ids) : ICommand<bool>;
