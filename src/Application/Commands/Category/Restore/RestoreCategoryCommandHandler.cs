using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Category.Restore;

public class RestoreCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<RestoreCategoryCommand, bool>
{
	public async ValueTask<bool> Handle(RestoreCategoryCommand request, CancellationToken cancellationToken)
	{
		return await categoryService.RestoreAsync(request.Id, cancellationToken);
	}
}
