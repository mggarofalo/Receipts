using Application.Interfaces;

namespace Application.Queries.Receipt;

public record GetAllReceiptsQuery() : IQuery<List<Domain.Core.Receipt>>;
