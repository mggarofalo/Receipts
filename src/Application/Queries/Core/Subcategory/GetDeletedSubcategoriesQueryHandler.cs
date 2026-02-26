using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Subcategory;

public class GetDeletedSubcategoriesQueryHandler(ISubcategoryService subcategoryService) : IRequestHandler<GetDeletedSubcategoriesQuery, List<Domain.Core.Subcategory>>
{
	public async Task<List<Domain.Core.Subcategory>> Handle(GetDeletedSubcategoriesQuery request, CancellationToken cancellationToken)
	{
		return await subcategoryService.GetDeletedAsync(cancellationToken);
	}
}
