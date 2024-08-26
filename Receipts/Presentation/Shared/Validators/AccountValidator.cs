using FluentValidation;
using Shared.ViewModels;

namespace Shared.Validators;

public class AccountValidator : AbstractValidator<AccountVM>
{
	public AccountValidator()
	{
		RuleFor(x => x.AccountCode)
			.NotEmpty()
			.WithMessage("Account code is required.");
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Name is required.");
	}
}