using Application.Interfaces;

namespace Application.Queries.Core.Account;

public record GetDeletedAccountsQuery() : IQuery<List<Domain.Core.Account>>;
