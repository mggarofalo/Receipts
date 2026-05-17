using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.ItemTemplate;

public class GetDeletedItemTemplatesQueryHandler(IItemTemplateService itemTemplateService) : IRequestHandler<GetDeletedItemTemplatesQuery, PagedResult<Domain.Core.ItemTemplate>>
{
	public async ValueTask<PagedResult<Domain.Core.ItemTemplate>> Handle(GetDeletedItemTemplatesQuery request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
