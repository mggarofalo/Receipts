using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.ItemTemplate;

public class GetItemTemplateByIdQueryHandler(IItemTemplateService itemTemplateService) : IRequestHandler<GetItemTemplateByIdQuery, Domain.Core.ItemTemplate?>
{
	public async ValueTask<Domain.Core.ItemTemplate?> Handle(GetItemTemplateByIdQuery request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.GetByIdAsync(request.Id, cancellationToken);
	}
}
