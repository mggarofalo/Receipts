using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Account;

public class GetAccountByIdQueryHandler(IAccountService accountService) : IRequestHandler<GetAccountByIdQuery, Domain.Core.Account?>
{
	public async ValueTask<Domain.Core.Account?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
	{
		return await accountService.GetByIdAsync(request.Id, cancellationToken);
	}
}
