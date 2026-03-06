using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class CreateSubcategoryRequestValidator : AbstractValidator<CreateSubcategoryRequest>
{
	public const string NameMustNotBeEmpty = "Name must not be empty.";
	public const string CategoryIdMustNotBeEmpty = "Category ID must not be empty.";

	public CreateSubcategoryRequestValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);

		RuleFor(x => x.CategoryId)
			.NotEqual(Guid.Empty)
			.WithMessage(CategoryIdMustNotBeEmpty);
	}
}
