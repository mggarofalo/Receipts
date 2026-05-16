using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetUnmappedCategoriesQueryHandler(IYnabCategoryMappingService service) : IRequestHandler<GetUnmappedCategoriesQuery, List<string>>
{
	public async ValueTask<List<string>> Handle(GetUnmappedCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await service.GetUnmappedCategoriesAsync(cancellationToken);
	}
}
