using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
	public const string AccountCodeMustNotBeEmpty = "Account code must not be empty.";
	public const string NameMustNotBeEmpty = "Name must not be empty.";

	public CreateAccountRequestValidator()
	{
		RuleFor(x => x.AccountCode)
			.NotEmpty()
			.WithMessage(AccountCodeMustNotBeEmpty);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);
	}
}
