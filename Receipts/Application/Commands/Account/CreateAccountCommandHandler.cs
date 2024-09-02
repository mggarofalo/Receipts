using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Account;

public class CreateAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<CreateAccountCommand, List<Domain.Core.Account>>
{
	public async Task<List<Domain.Core.Account>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Account> createdEntities = await accountRepository.CreateAsync([.. request.Accounts], cancellationToken);
		await accountRepository.SaveChangesAsync(cancellationToken);
		return createdEntities;
	}
}