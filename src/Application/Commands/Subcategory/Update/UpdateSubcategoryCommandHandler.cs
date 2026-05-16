using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Subcategory.Update;

public class UpdateSubcategoryCommandHandler(ISubcategoryService subcategoryService) : IRequestHandler<UpdateSubcategoryCommand, bool>
{
	public async ValueTask<bool> Handle(UpdateSubcategoryCommand request, CancellationToken cancellationToken)
	{
		await subcategoryService.UpdateAsync([.. request.Subcategories], cancellationToken);
		return true;
	}
}
