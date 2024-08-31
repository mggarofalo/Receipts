using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Account;

public class DeleteAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<DeleteAccountCommand, bool>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
	{
		bool success = await _accountRepository.DeleteAsync([.. request.Ids], cancellationToken);

		if (success)
		{
			await _accountRepository.SaveChangesAsync(cancellationToken);
		}

		return success;
	}
}