using Application.Interfaces;

namespace Application.Commands.Transaction;

public record DeleteTransactionCommand(List<Guid> Ids) : ICommand<bool>;
