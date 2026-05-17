using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Subcategory.Restore;

public class RestoreSubcategoryCommandHandler(ISubcategoryService subcategoryService) : IRequestHandler<RestoreSubcategoryCommand, bool>
{
	public async ValueTask<bool> Handle(RestoreSubcategoryCommand request, CancellationToken cancellationToken)
	{
		return await subcategoryService.RestoreAsync(request.Id, cancellationToken);
	}
}
