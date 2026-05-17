using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.ItemTemplate.Restore;

public class RestoreItemTemplateCommandHandler(IItemTemplateService itemTemplateService) : IRequestHandler<RestoreItemTemplateCommand, bool>
{
	public async ValueTask<bool> Handle(RestoreItemTemplateCommand request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.RestoreAsync(request.Id, cancellationToken);
	}
}
