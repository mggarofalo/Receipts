using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Account;

public class GetAccountByIdQueryHandler(IAccountService accountRepository) : IRequestHandler<GetAccountByIdQuery, Domain.Core.Account?>
{
	public async Task<Domain.Core.Account?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
	{
		return await accountRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}