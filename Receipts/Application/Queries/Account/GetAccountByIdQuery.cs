using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Account;

public record GetAccountByIdQuery(Guid Id) : IQuery<Domain.Core.Account?>;

public class GetAccountByIdQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAccountByIdQuery, Domain.Core.Account?>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<Domain.Core.Account?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
	{
		return await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}