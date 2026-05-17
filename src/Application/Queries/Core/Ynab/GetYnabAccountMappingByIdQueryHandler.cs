using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetYnabAccountMappingByIdQueryHandler(IYnabAccountMappingService accountMappingService) : IRequestHandler<GetYnabAccountMappingByIdQuery, YnabAccountMappingDto?>
{
	public async ValueTask<YnabAccountMappingDto?> Handle(GetYnabAccountMappingByIdQuery request, CancellationToken cancellationToken)
	{
		return await accountMappingService.GetByIdAsync(request.Id, cancellationToken);
	}
}
