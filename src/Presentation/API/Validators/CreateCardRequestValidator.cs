using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class CreateCardRequestValidator : AbstractValidator<CreateCardRequest>
{
	public const string CardCodeMustNotBeEmpty = "Card code must not be empty.";
	public const string NameMustNotBeEmpty = "Name must not be empty.";

	public CreateCardRequestValidator()
	{
		RuleFor(x => x.CardCode)
			.NotEmpty()
			.WithMessage(CardCodeMustNotBeEmpty);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);
	}
}
