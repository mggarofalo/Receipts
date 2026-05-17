using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.ItemTemplate.Delete;

public class DeleteItemTemplateCommandHandler(IItemTemplateService itemTemplateService) : IRequestHandler<DeleteItemTemplateCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteItemTemplateCommand request, CancellationToken cancellationToken)
	{
		await itemTemplateService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
