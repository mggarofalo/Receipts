using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Subcategory;

public class GetDeletedSubcategoriesQueryHandler(ISubcategoryService subcategoryService) : IRequestHandler<GetDeletedSubcategoriesQuery, PagedResult<Domain.Core.Subcategory>>
{
	public async Task<PagedResult<Domain.Core.Subcategory>> Handle(GetDeletedSubcategoriesQuery request, CancellationToken cancellationToken)
	{
		return await subcategoryService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
