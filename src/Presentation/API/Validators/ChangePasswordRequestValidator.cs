using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
	public ChangePasswordRequestValidator()
	{
		RuleFor(x => x.CurrentPassword)
			.NotEmpty();

		RuleFor(x => x.NewPassword)
			.NotEmpty()
			.MinimumLength(8)
			.NotEqual(x => x.CurrentPassword)
			.WithMessage("New password must be different from current password.");
	}
}
