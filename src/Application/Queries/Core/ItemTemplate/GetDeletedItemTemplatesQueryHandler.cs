using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.ItemTemplate;

public class GetDeletedItemTemplatesQueryHandler(IItemTemplateService itemTemplateService) : IRequestHandler<GetDeletedItemTemplatesQuery, PagedResult<Domain.Core.ItemTemplate>>
{
	public async Task<PagedResult<Domain.Core.ItemTemplate>> Handle(GetDeletedItemTemplatesQuery request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.GetDeletedAsync(request.Offset, request.Limit, cancellationToken);
	}
}
