using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Account;

public record GetAllAccountsQuery() : IQuery<List<Domain.Core.Account>>;

public class GetAllAccountsQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAllAccountsQuery, List<Domain.Core.Account>>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<List<Domain.Core.Account>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
	{
		return await _accountRepository.GetAllAsync(cancellationToken);
	}
}