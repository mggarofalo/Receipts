using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Commands.Account;

public record DeleteAccountCommand(Guid Id) : ICommand<bool>;

public class DeleteAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<DeleteAccountCommand, bool>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
	{
		bool success = await _accountRepository.DeleteAsync(request.Id, cancellationToken);

		if (success)
		{
			await _accountRepository.SaveChangesAsync(cancellationToken);
			return true;
		}

		return false;
	}
}