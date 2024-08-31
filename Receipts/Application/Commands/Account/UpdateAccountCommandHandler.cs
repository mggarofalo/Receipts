using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Account;

public class UpdateAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<UpdateAccountCommand, bool>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
	{
		bool success = await _accountRepository.UpdateAsync(request.Accounts, cancellationToken);

		if (success)
		{
			await _accountRepository.SaveChangesAsync(cancellationToken);
		}

		return success;
	}
}