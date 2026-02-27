using Application.Interfaces;

namespace Application.Queries.Core.Subcategory;

public record GetAllSubcategoriesQuery() : IQuery<List<Domain.Core.Subcategory>>;
