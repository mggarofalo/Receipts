using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Category;

public class GetAllCategoriesQueryHandler(ICategoryService categoryService) : IRequestHandler<GetAllCategoriesQuery, PagedResult<Domain.Core.Category>>
{
	public async Task<PagedResult<Domain.Core.Category>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await categoryService.GetAllAsync(request.Offset, request.Limit, cancellationToken);
	}
}
