using Application.Interfaces;

namespace Application.Queries.Core.Receipt;

public record GetAllReceiptsQuery() : IQuery<List<Domain.Core.Receipt>>;
