using Application.Interfaces;

namespace Application.Queries.Core.Account;

public record GetAllAccountsQuery() : IQuery<List<Domain.Core.Account>>;
