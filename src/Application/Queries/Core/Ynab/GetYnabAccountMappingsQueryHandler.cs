using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetYnabAccountMappingsQueryHandler(IYnabAccountMappingService accountMappingService) : IRequestHandler<GetYnabAccountMappingsQuery, List<YnabAccountMappingDto>>
{
	public async ValueTask<List<YnabAccountMappingDto>> Handle(GetYnabAccountMappingsQuery request, CancellationToken cancellationToken)
	{
		return await accountMappingService.GetAllAsync(cancellationToken);
	}
}
