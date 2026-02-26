using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ItemTemplate.Update;

public class UpdateItemTemplateCommandHandler(IItemTemplateService itemTemplateService) : IRequestHandler<UpdateItemTemplateCommand, bool>
{
	public async Task<bool> Handle(UpdateItemTemplateCommand request, CancellationToken cancellationToken)
	{
		await itemTemplateService.UpdateAsync([.. request.ItemTemplates], cancellationToken);
		return true;
	}
}
