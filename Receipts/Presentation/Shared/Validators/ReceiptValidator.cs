using FluentValidation;
using Shared.ViewModels.Core;

namespace Shared.Validators;

public class ReceiptValidator : AbstractValidator<ReceiptVM>
{
	public ReceiptValidator()
	{
		RuleFor(x => x.Description)
			.MaximumLength(256)
			.WithMessage("Description must not exceed 256 characters.");

		RuleFor(x => x.Location)
			.NotEmpty()
			.MaximumLength(200)
			.WithMessage("Location must not exceed 200 characters.");

		RuleFor(x => x.Date.ToDateTime(new TimeOnly()))
			.NotEmpty()
			.LessThanOrEqualTo(DateTime.Today)
			.WithMessage("Date must be prior to the current date");
	}
}