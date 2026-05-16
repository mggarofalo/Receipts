using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Subcategory.Delete;

public class DeleteSubcategoryCommandHandler(ISubcategoryService subcategoryService) : IRequestHandler<DeleteSubcategoryCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteSubcategoryCommand request, CancellationToken cancellationToken)
	{
		await subcategoryService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
