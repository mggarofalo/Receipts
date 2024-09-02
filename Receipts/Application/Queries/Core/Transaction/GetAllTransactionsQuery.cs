using Application.Interfaces;

namespace Application.Queries.Core.Transaction;

public record GetAllTransactionsQuery() : IQuery<List<Domain.Core.Transaction>>;
