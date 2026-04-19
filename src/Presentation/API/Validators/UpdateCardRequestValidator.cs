using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class UpdateCardRequestValidator : AbstractValidator<UpdateCardRequest>
{
	public const string IdMustNotBeEmpty = "ID must not be empty.";
	public const string CardCodeMustNotBeEmpty = "Card code must not be empty.";
	public const string NameMustNotBeEmpty = "Name must not be empty.";

	public UpdateCardRequestValidator()
	{
		RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage(IdMustNotBeEmpty);

		RuleFor(x => x.CardCode)
			.NotEmpty()
			.WithMessage(CardCodeMustNotBeEmpty);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);
	}
}
