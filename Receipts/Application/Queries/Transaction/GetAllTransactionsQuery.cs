using Application.Interfaces;

namespace Application.Queries.Transaction;

public record GetAllTransactionsQuery() : IQuery<List<Domain.Core.Transaction>>;
