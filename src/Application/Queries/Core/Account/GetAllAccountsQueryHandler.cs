using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Account;

public class GetAllAccountsQueryHandler(IAccountService accountService) : IRequestHandler<GetAllAccountsQuery, PagedResult<Domain.Core.Account>>
{
	public async Task<PagedResult<Domain.Core.Account>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
	{
		return await accountService.GetAllAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
