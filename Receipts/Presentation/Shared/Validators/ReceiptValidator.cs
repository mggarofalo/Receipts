using FluentValidation;
using Shared.ViewModels;

namespace Shared.Validators;

public class ReceiptValidator : AbstractValidator<ReceiptVM>
{
	public ReceiptValidator()
	{
		RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Date).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow);
		RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
		RuleFor(x => x.TotalAmount).GreaterThan(0);
		RuleFor(x => x.Description).MaximumLength(256);
	}
}
