using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetYnabRateLimitStatusQueryHandler(IYnabRateLimitTracker rateLimitTracker) : IRequestHandler<GetYnabRateLimitStatusQuery, YnabRateLimitStatus>
{
	public ValueTask<YnabRateLimitStatus> Handle(GetYnabRateLimitStatusQuery request, CancellationToken cancellationToken)
	{
		return ValueTask.FromResult(rateLimitTracker.GetStatus());
	}
}
