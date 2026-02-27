using Application.Interfaces;

namespace Application.Queries.Core.Subcategory;

public record GetDeletedSubcategoriesQuery() : IQuery<List<Domain.Core.Subcategory>>;
