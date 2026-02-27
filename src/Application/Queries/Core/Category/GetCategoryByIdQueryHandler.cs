using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Category;

public class GetCategoryByIdQueryHandler(ICategoryService categoryService) : IRequestHandler<GetCategoryByIdQuery, Domain.Core.Category?>
{
	public async Task<Domain.Core.Category?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
	{
		return await categoryService.GetByIdAsync(request.Id, cancellationToken);
	}
}
