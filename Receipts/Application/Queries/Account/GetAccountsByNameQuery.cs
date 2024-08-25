using Application.Interfaces;
using MediatR;

namespace Application.Queries.Account;

public record GetAccountsByNameQuery(string Name) : IQuery<List<Domain.Core.Account>>;

public class GetAccountsByNameQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAccountsByNameQuery, List<Domain.Core.Account>>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<List<Domain.Core.Account>> Handle(GetAccountsByNameQuery request, CancellationToken cancellationToken)
	{
		return await _accountRepository.GetByNameAsync(request.Name, cancellationToken);
	}
}