using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Transaction;

public record GetDeletedTransactionsQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Transaction>>;
