using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Subcategory;

public record GetDeletedSubcategoriesQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Subcategory>>;
