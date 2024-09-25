using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Core.Account;

public class GetAllAccountsQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAllAccountsQuery, List<Domain.Core.Account>>
{
	public async Task<List<Domain.Core.Account>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
	{
		return await accountRepository.GetAllAsync(cancellationToken);
	}
}