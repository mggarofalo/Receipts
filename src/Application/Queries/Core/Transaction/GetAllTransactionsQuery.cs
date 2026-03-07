using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Transaction;

public record GetAllTransactionsQuery(int Offset, int Limit, SortParams Sort) : IQuery<PagedResult<Domain.Core.Transaction>>;
