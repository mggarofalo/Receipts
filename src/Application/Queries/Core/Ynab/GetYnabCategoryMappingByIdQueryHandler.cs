using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetYnabCategoryMappingByIdQueryHandler(IYnabCategoryMappingService service) : IRequestHandler<GetYnabCategoryMappingByIdQuery, YnabCategoryMappingDto?>
{
	public async ValueTask<YnabCategoryMappingDto?> Handle(GetYnabCategoryMappingByIdQuery request, CancellationToken cancellationToken)
	{
		return await service.GetByIdAsync(request.Id, cancellationToken);
	}
}
