using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Account;

public class GetDeletedAccountsQueryHandler(IAccountService accountService) : IRequestHandler<GetDeletedAccountsQuery, List<Domain.Core.Account>>
{
	public async Task<List<Domain.Core.Account>> Handle(GetDeletedAccountsQuery request, CancellationToken cancellationToken)
	{
		return await accountService.GetDeletedAsync(cancellationToken);
	}
}
