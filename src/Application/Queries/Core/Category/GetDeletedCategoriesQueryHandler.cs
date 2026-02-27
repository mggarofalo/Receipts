using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Category;

public class GetDeletedCategoriesQueryHandler(ICategoryService categoryService) : IRequestHandler<GetDeletedCategoriesQuery, List<Domain.Core.Category>>
{
	public async Task<List<Domain.Core.Category>> Handle(GetDeletedCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await categoryService.GetDeletedAsync(cancellationToken);
	}
}
