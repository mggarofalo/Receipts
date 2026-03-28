using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Category.Delete;

public class DeleteCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand, bool>
{
	public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
	{
		bool exists = await categoryService.ExistsAsync(request.Id, cancellationToken);
		if (!exists)
		{
			return false;
		}

		await categoryService.DeleteAsync(request.Id, cancellationToken);
		return true;
	}
}
