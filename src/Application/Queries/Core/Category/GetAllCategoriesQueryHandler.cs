using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Category;

public class GetAllCategoriesQueryHandler(ICategoryService categoryService) : IRequestHandler<GetAllCategoriesQuery, List<Domain.Core.Category>>
{
	public async Task<List<Domain.Core.Category>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await categoryService.GetAllAsync(cancellationToken);
	}
}
