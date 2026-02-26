using API.Generated.Dtos;
using Common;
using FluentValidation;

namespace API.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress();

		RuleFor(x => x.Password)
			.NotEmpty()
			.MinimumLength(8);

		RuleFor(x => x.Role)
			.NotEmpty()
			.Must(role => AppRoles.All.Contains(role))
			.WithMessage($"Role must be one of: {string.Join(", ", AppRoles.All)}");
	}
}
