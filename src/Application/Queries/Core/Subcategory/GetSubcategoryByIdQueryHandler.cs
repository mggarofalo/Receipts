using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Subcategory;

public class GetSubcategoryByIdQueryHandler(ISubcategoryService subcategoryService) : IRequestHandler<GetSubcategoryByIdQuery, Domain.Core.Subcategory?>
{
	public async ValueTask<Domain.Core.Subcategory?> Handle(GetSubcategoryByIdQuery request, CancellationToken cancellationToken)
	{
		return await subcategoryService.GetByIdAsync(request.Id, cancellationToken);
	}
}
