using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class AdminResetPasswordRequestValidator : AbstractValidator<AdminResetPasswordRequest>
{
	public AdminResetPasswordRequestValidator()
	{
		RuleFor(x => x.NewPassword)
			.NotEmpty()
			.MinimumLength(8);
	}
}
