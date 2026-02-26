using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Subcategory;

public class GetAllSubcategoriesQueryHandler(ISubcategoryService subcategoryService) : IRequestHandler<GetAllSubcategoriesQuery, List<Domain.Core.Subcategory>>
{
	public async Task<List<Domain.Core.Subcategory>> Handle(GetAllSubcategoriesQuery request, CancellationToken cancellationToken)
	{
		return await subcategoryService.GetAllAsync(cancellationToken);
	}
}
