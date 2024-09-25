using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Account;

public class DeleteAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<DeleteAccountCommand, bool>
{
	public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
	{
		await accountRepository.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}