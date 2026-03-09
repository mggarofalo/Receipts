using Application.Interfaces;
using Application.Queries.Core.ItemTemplate.GetSimilarItems;

namespace Application.Queries.Core.ItemTemplate.GetCategoryRecommendations;

public record GetCategoryRecommendationsQuery(string Description, int Limit = 5) : IQuery<IEnumerable<CategoryRecommendation>>;
