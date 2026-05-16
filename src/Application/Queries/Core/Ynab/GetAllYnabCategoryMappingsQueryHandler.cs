using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetAllYnabCategoryMappingsQueryHandler(IYnabCategoryMappingService service) : IRequestHandler<GetAllYnabCategoryMappingsQuery, List<YnabCategoryMappingDto>>
{
	public async ValueTask<List<YnabCategoryMappingDto>> Handle(GetAllYnabCategoryMappingsQuery request, CancellationToken cancellationToken)
	{
		return await service.GetAllAsync(cancellationToken);
	}
}
