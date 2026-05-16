using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetYnabBudgetsQueryHandler(IYnabApiClient ynabApiClient) : IRequestHandler<GetYnabBudgetsQuery, List<YnabBudget>>
{
	public async ValueTask<List<YnabBudget>> Handle(GetYnabBudgetsQuery request, CancellationToken cancellationToken)
	{
		return await ynabApiClient.GetBudgetsAsync(cancellationToken);
	}
}
