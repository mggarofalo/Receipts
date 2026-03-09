using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ItemTemplate.GetSimilarItems;

public class GetSimilarItemsQueryHandler(IItemTemplateSimilarityService similarityService) : IRequestHandler<GetSimilarItemsQuery, IEnumerable<SimilarItemResult>>
{
	public async Task<IEnumerable<SimilarItemResult>> Handle(GetSimilarItemsQuery request, CancellationToken cancellationToken)
	{
		return await similarityService.GetSimilarItemsAsync(request.SearchText, request.Limit, request.Threshold, cancellationToken);
	}
}
