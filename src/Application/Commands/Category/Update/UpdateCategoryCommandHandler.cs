using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Category.Update;

public class UpdateCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<UpdateCategoryCommand, bool>
{
	public async ValueTask<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
	{
		await categoryService.UpdateAsync([.. request.Categories], cancellationToken);
		return true;
	}
}
