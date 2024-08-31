using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Account;

public record CreateAccountCommand(List<Domain.Core.Account> Accounts) : ICommand<List<Domain.Core.Account>>;

public class CreateAccountCommandHandler(IAccountRepository accountRepository) : IRequestHandler<CreateAccountCommand, List<Domain.Core.Account>>
{
	private readonly IAccountRepository _accountRepository = accountRepository;

	public async Task<List<Domain.Core.Account>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Account> createdEntities = await _accountRepository.CreateAsync(request.Accounts, cancellationToken);
		await _accountRepository.SaveChangesAsync(cancellationToken);
		return createdEntities;
	}
}