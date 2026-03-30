using Application.Interfaces;

namespace Application.Queries.Core.Receipt;

public record GetDistinctLocationsQuery(string? Query, int Limit) : IQuery<List<string>>;
