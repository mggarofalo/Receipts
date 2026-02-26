using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Subcategory.Create;

public class CreateSubcategoryCommandHandler(ISubcategoryService subcategoryService) : IRequestHandler<CreateSubcategoryCommand, List<Domain.Core.Subcategory>>
{
	public async Task<List<Domain.Core.Subcategory>> Handle(CreateSubcategoryCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Subcategory> createdEntities = await subcategoryService.CreateAsync([.. request.Subcategories], cancellationToken);
		return createdEntities;
	}
}
