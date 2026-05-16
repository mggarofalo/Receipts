using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.ItemTemplate;

public class GetAllItemTemplatesQueryHandler(IItemTemplateService itemTemplateService) : IRequestHandler<GetAllItemTemplatesQuery, PagedResult<Domain.Core.ItemTemplate>>
{
	public async ValueTask<PagedResult<Domain.Core.ItemTemplate>> Handle(GetAllItemTemplatesQuery request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.GetAllAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
