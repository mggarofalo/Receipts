using Application.Interfaces;

namespace Application.Queries.Transaction;

public record GetTransactionByIdQuery(Guid Id) : IQuery<Domain.Core.Transaction?>;
