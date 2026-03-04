using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.Subcategory;

public record GetAllSubcategoriesQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.Subcategory>>;
