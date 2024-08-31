using Application.Interfaces;

namespace Application.Commands.Transaction;

public record CreateTransactionCommand(List<Domain.Core.Transaction> Transactions) : ICommand<List<Domain.Core.Transaction>>;
