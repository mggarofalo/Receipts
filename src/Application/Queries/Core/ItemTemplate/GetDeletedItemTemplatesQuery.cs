using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.ItemTemplate;

public record GetDeletedItemTemplatesQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.ItemTemplate>>;
