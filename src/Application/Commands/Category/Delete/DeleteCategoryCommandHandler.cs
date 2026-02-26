using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Category.Delete;

public class DeleteCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand, bool>
{
	public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
	{
		await categoryService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
