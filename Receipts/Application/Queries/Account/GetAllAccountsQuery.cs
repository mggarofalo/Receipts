using Application.Interfaces;

namespace Application.Queries.Account;

public record GetAllAccountsQuery() : IQuery<List<Domain.Core.Account>>;
