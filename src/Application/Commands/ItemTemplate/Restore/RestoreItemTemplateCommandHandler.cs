using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ItemTemplate.Restore;

public class RestoreItemTemplateCommandHandler(IItemTemplateService itemTemplateService) : IRequestHandler<RestoreItemTemplateCommand, bool>
{
	public async Task<bool> Handle(RestoreItemTemplateCommand request, CancellationToken cancellationToken)
	{
		return await itemTemplateService.RestoreAsync(request.Id, cancellationToken);
	}
}
