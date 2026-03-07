using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Account;

public class GetDeletedAccountsQueryHandler(IAccountService accountService) : IRequestHandler<GetDeletedAccountsQuery, PagedResult<Domain.Core.Account>>
{
	public async Task<PagedResult<Domain.Core.Account>> Handle(GetDeletedAccountsQuery request, CancellationToken cancellationToken)
	{
		return await accountService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
