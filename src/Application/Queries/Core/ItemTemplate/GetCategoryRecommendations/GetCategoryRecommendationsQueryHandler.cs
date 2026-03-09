using Application.Interfaces.Services;
using Application.Queries.Core.ItemTemplate.GetSimilarItems;
using MediatR;

namespace Application.Queries.Core.ItemTemplate.GetCategoryRecommendations;

public class GetCategoryRecommendationsQueryHandler(IItemTemplateSimilarityService similarityService) : IRequestHandler<GetCategoryRecommendationsQuery, IEnumerable<CategoryRecommendation>>
{
	public async Task<IEnumerable<CategoryRecommendation>> Handle(GetCategoryRecommendationsQuery request, CancellationToken cancellationToken)
	{
		return await similarityService.GetCategoryRecommendationsAsync(request.Description, request.Limit, cancellationToken);
	}
}
