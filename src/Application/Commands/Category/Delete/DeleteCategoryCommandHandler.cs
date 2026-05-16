using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Category.Delete;

public class DeleteCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
	{
		await categoryService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
