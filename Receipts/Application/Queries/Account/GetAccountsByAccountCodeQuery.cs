using Application.Interfaces;
using MediatR;

namespace Application.Queries.Account;

public record GetAccountsByAccountCodeQuery(string AccountCode) : IQuery<List<Domain.Core.Account>>;

public class GetAccountsByAccountCodeQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAccountsByAccountCodeQuery, List<Domain.Core.Account>>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<List<Domain.Core.Account>> Handle(GetAccountsByAccountCodeQuery request, CancellationToken cancellationToken)
	{
		return await _accountRepository.GetByAccountCodeAsync(request.AccountCode, cancellationToken);
	}
}