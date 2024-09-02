using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Account;

public class GetAccountByIdQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAccountByIdQuery, Domain.Core.Account?>
{
	public async Task<Domain.Core.Account?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
	{
		return await accountRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}