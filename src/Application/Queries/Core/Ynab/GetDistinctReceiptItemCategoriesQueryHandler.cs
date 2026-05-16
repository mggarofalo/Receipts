using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetDistinctReceiptItemCategoriesQueryHandler(IYnabCategoryMappingService service) : IRequestHandler<GetDistinctReceiptItemCategoriesQuery, List<string>>
{
	public async ValueTask<List<string>> Handle(GetDistinctReceiptItemCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await service.GetDistinctReceiptItemCategoriesAsync(cancellationToken);
	}
}
