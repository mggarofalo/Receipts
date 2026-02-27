using Application.Interfaces;

namespace Application.Queries.Core.Category;

public record GetDeletedCategoriesQuery() : IQuery<List<Domain.Core.Category>>;
