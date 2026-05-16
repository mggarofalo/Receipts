using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.ItemTemplate.Create;

public class CreateItemTemplateCommandHandler(IItemTemplateService itemTemplateService) : IRequestHandler<CreateItemTemplateCommand, List<Domain.Core.ItemTemplate>>
{
	public async ValueTask<List<Domain.Core.ItemTemplate>> Handle(CreateItemTemplateCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.ItemTemplate> createdEntities = await itemTemplateService.CreateAsync([.. request.ItemTemplates], cancellationToken);
		return createdEntities;
	}
}
