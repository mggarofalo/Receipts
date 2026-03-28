using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Category;

public class GetDeletedCategoriesQueryHandler(ICategoryService categoryService) : IRequestHandler<GetDeletedCategoriesQuery, PagedResult<Domain.Core.Category>>
{
	public async Task<PagedResult<Domain.Core.Category>> Handle(GetDeletedCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await categoryService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
