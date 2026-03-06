using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class UpdateSubcategoryRequestValidator : AbstractValidator<UpdateSubcategoryRequest>
{
	public const string IdMustNotBeEmpty = "ID must not be empty.";
	public const string NameMustNotBeEmpty = "Name must not be empty.";
	public const string CategoryIdMustNotBeEmpty = "Category ID must not be empty.";

	public UpdateSubcategoryRequestValidator()
	{
		RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage(IdMustNotBeEmpty);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);

		RuleFor(x => x.CategoryId)
			.NotEqual(Guid.Empty)
			.WithMessage(CategoryIdMustNotBeEmpty);
	}
}
