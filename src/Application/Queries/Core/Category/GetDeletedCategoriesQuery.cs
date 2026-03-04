using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Category;

public record GetDeletedCategoriesQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Category>>;
