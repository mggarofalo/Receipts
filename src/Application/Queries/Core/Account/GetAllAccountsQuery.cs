using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Account;

public record GetAllAccountsQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Account>>;
