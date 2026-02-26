using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.ItemTemplate.Delete;

public class DeleteItemTemplateCommandHandler(IItemTemplateService itemTemplateService) : IRequestHandler<DeleteItemTemplateCommand, bool>
{
	public async Task<bool> Handle(DeleteItemTemplateCommand request, CancellationToken cancellationToken)
	{
		await itemTemplateService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
