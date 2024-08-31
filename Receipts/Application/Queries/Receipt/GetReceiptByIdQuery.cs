using Application.Interfaces;

namespace Application.Queries.Receipt;

public record GetReceiptByIdQuery(Guid Id) : IQuery<Domain.Core.Receipt?>;
