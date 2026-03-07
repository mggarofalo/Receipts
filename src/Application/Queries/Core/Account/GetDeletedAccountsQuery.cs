using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Account;

public record GetDeletedAccountsQuery(int Offset, int Limit, SortParams Sort) : IQuery<PagedResult<Domain.Core.Account>>;
