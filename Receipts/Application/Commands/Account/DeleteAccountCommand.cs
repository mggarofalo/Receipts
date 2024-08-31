using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Account;

public record DeleteAccountCommand(List<Guid> Ids) : ICommand<bool>;

public class DeleteAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<DeleteAccountCommand, bool>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
	{
		bool success = await _accountRepository.DeleteAsync(request.Ids, cancellationToken);

		if (success)
		{
			await _accountRepository.SaveChangesAsync(cancellationToken);
		}

		return success;
	}
}