using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ItemTemplate;

public class GetDeletedItemTemplatesQueryHandler(IItemTemplateService itemTemplateService) : IRequestHandler<GetDeletedItemTemplatesQuery, List<Domain.Core.ItemTemplate>>
{
	public async Task<List<Domain.Core.ItemTemplate>> Handle(GetDeletedItemTemplatesQuery request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.GetDeletedAsync(cancellationToken);
	}
}
