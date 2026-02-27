using Application.Interfaces;

namespace Application.Queries.Core.ItemTemplate;

public record GetDeletedItemTemplatesQuery() : IQuery<List<Domain.Core.ItemTemplate>>;
