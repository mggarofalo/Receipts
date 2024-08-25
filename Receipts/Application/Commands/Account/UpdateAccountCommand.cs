using Application.Common;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Commands.Account;

public record UpdateAccountCommand(Guid Id, string AccountCode, string Name, bool IsActive) : ICommand<bool>;

public class UpdateAccountCommandHandler(IAccountRepository accountRepository, IMapper mapper) : IRequestHandler<UpdateAccountCommand, bool>
{
	private readonly IAccountRepository _accountRepository = accountRepository;
	private readonly IMapper _mapper = mapper;

	public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
	{
		Domain.Core.Account? existingAccount = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);

		if (existingAccount == null)
		{
			return false;
		}

		_mapper.Map(request, existingAccount);

		bool success = await _accountRepository.UpdateAsync(existingAccount, cancellationToken);

		if (success)
		{
			await _accountRepository.SaveChangesAsync(cancellationToken);
			return true;
		}

		return false;
	}
}