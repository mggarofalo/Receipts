using API.Generated.Dtos;
using Common;
using FluentValidation;

namespace API.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
	public UpdateUserRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress();

		RuleFor(x => x.Role)
			.NotEmpty()
			.Must(role => AppRoles.All.Contains(role))
			.WithMessage($"Role must be one of: {string.Join(", ", AppRoles.All)}");
	}
}
