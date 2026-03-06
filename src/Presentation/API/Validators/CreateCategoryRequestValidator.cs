using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
	public const string NameMustNotBeEmpty = "Name must not be empty.";

	public CreateCategoryRequestValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);
	}
}
