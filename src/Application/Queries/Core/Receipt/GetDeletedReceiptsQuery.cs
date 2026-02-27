using Application.Interfaces;

namespace Application.Queries.Core.Receipt;

public record GetDeletedReceiptsQuery() : IQuery<List<Domain.Core.Receipt>>;
