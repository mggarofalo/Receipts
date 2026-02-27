using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ItemTemplate;

public class GetAllItemTemplatesQueryHandler(IItemTemplateService itemTemplateService) : IRequestHandler<GetAllItemTemplatesQuery, List<Domain.Core.ItemTemplate>>
{
	public async Task<List<Domain.Core.ItemTemplate>> Handle(GetAllItemTemplatesQuery request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.GetAllAsync(cancellationToken);
	}
}
