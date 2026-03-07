using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.ItemTemplate;

public record GetDeletedItemTemplatesQuery(int Offset, int Limit, SortParams Sort) : IQuery<PagedResult<Domain.Core.ItemTemplate>>;
