using FluentValidation;
using Shared.ViewModels.Core;

namespace Shared.Validators;

public class AccountValidator : AbstractValidator<AccountVM>
{
	public const string AccountCodeIsRequired = "Account code is required.";
	public const string NameIsRequired = "Name is required.";

	public AccountValidator()
	{
		RuleFor(x => x.AccountCode)
			.NotEmpty()
			.WithMessage(AccountCodeIsRequired);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameIsRequired);
	}
}