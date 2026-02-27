using Application.Interfaces;

namespace Application.Queries.Core.Category;

public record GetAllCategoriesQuery() : IQuery<List<Domain.Core.Category>>;
