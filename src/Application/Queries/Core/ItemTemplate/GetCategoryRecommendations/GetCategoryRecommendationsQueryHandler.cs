using Application.Interfaces.Services;
using Application.Queries.Core.ItemTemplate.GetSimilarItems;
using Mediator;

namespace Application.Queries.Core.ItemTemplate.GetCategoryRecommendations;

public class GetCategoryRecommendationsQueryHandler(IItemTemplateSimilarityService similarityService) : IRequestHandler<GetCategoryRecommendationsQuery, IEnumerable<CategoryRecommendation>>
{
	public async ValueTask<IEnumerable<CategoryRecommendation>> Handle(GetCategoryRecommendationsQuery request, CancellationToken cancellationToken)
	{
		return await similarityService.GetCategoryRecommendationsAsync(request.Description, request.Limit, cancellationToken);
	}
}
