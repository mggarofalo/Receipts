using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Subcategory.Update;

public class UpdateSubcategoryCommandHandler(ISubcategoryService subcategoryService) : IRequestHandler<UpdateSubcategoryCommand, bool>
{
	public async Task<bool> Handle(UpdateSubcategoryCommand request, CancellationToken cancellationToken)
	{
		await subcategoryService.UpdateAsync([.. request.Subcategories], cancellationToken);
		return true;
	}
}
