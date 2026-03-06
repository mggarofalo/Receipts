using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
	public const string IdMustNotBeEmpty = "ID must not be empty.";
	public const string AccountCodeMustNotBeEmpty = "Account code must not be empty.";
	public const string NameMustNotBeEmpty = "Name must not be empty.";

	public UpdateAccountRequestValidator()
	{
		RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage(IdMustNotBeEmpty);

		RuleFor(x => x.AccountCode)
			.NotEmpty()
			.WithMessage(AccountCodeMustNotBeEmpty);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);
	}
}
