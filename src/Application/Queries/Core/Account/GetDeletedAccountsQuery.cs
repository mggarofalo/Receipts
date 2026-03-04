using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Account;

public record GetDeletedAccountsQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Account>>;
