using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetYnabCategoriesQueryHandler(IYnabApiClient ynabApiClient) : IRequestHandler<GetYnabCategoriesQuery, List<YnabCategory>>
{
	public async ValueTask<List<YnabCategory>> Handle(GetYnabCategoriesQuery request, CancellationToken cancellationToken)
	{
		return await ynabApiClient.GetCategoriesAsync(request.BudgetId, cancellationToken);
	}
}
