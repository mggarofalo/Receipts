using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.Category;

public class GetDeletedCategoriesQueryHandler(ICategoryService categoryService) : IRequestHandler<GetDeletedCategoriesQuery, PagedResult<Domain.Core.Category>>
{
	public async ValueTask<PagedResult<Domain.Core.Category>> Handle(GetDeletedCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await categoryService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
