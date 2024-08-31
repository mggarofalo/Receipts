using Application.Interfaces;

namespace Application.Commands.Transaction;

public record UpdateTransactionCommand(List<Domain.Core.Transaction> Transactions) : ICommand<bool>;
