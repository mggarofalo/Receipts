using Application.Interfaces;

namespace Application.Queries.Core.ItemTemplate;

public record GetAllItemTemplatesQuery() : IQuery<List<Domain.Core.ItemTemplate>>;
