using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Subcategory;

public class GetSubcategoriesByCategoryIdQueryHandler(ISubcategoryService subcategoryService) : IRequestHandler<GetSubcategoriesByCategoryIdQuery, List<Domain.Core.Subcategory>>
{
	public async Task<List<Domain.Core.Subcategory>> Handle(GetSubcategoriesByCategoryIdQuery request, CancellationToken cancellationToken)
	{
		return await subcategoryService.GetByCategoryIdAsync(request.CategoryId, cancellationToken);
	}
}
