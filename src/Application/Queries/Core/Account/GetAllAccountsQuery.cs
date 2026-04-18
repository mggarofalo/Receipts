using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Account;

public record GetAllAccountsQuery(int Offset, int Limit, SortParams Sort, bool? IsActive = null) : IQuery<PagedResult<Domain.Core.Account>>;
