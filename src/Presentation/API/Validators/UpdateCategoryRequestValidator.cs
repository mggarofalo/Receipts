using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
	public const string IdMustNotBeEmpty = "ID must not be empty.";
	public const string NameMustNotBeEmpty = "Name must not be empty.";

	public UpdateCategoryRequestValidator()
	{
		RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage(IdMustNotBeEmpty);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage(NameMustNotBeEmpty);
	}
}
