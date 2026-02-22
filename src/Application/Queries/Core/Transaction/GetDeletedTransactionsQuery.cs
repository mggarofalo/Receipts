using Application.Interfaces;

namespace Application.Queries.Core.Transaction;

public record GetDeletedTransactionsQuery() : IQuery<List<Domain.Core.Transaction>>;
