using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Account;

public class UpdateAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<UpdateAccountCommand, bool>
{
	public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
	{
		await accountRepository.UpdateAsync([.. request.Accounts], cancellationToken);
		return true;
	}
}