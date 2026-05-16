using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.ItemTemplate.Update;

public class UpdateItemTemplateCommandHandler(IItemTemplateService itemTemplateService) : IRequestHandler<UpdateItemTemplateCommand, bool>
{
	public async ValueTask<bool> Handle(UpdateItemTemplateCommand request, CancellationToken cancellationToken)
	{
		await itemTemplateService.UpdateAsync([.. request.ItemTemplates], cancellationToken);
		return true;
	}
}
