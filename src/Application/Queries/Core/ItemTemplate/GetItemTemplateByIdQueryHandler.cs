using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ItemTemplate;

public class GetItemTemplateByIdQueryHandler(IItemTemplateService itemTemplateService) : IRequestHandler<GetItemTemplateByIdQuery, Domain.Core.ItemTemplate?>
{
	public async Task<Domain.Core.ItemTemplate?> Handle(GetItemTemplateByIdQuery request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.GetByIdAsync(request.Id, cancellationToken);
	}
}
