using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Category.Create;

public class CreateCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<CreateCategoryCommand, List<Domain.Core.Category>>
{
	public async Task<List<Domain.Core.Category>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Category> createdEntities = await categoryService.CreateAsync([.. request.Categories], cancellationToken);
		return createdEntities;
	}
}
