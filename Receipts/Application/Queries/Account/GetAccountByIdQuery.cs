using Application.Interfaces;

namespace Application.Queries.Account;

public record GetAccountByIdQuery(Guid Id) : IQuery<Domain.Core.Account?>;
