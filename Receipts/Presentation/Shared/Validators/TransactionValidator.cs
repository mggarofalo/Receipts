using FluentValidation;
using Shared.ViewModels;

namespace Shared.Validators;

public class TransactionValidator : AbstractValidator<TransactionVM>
{
	public TransactionValidator()
	{
		RuleFor(x => x.Amount)
			.NotEmpty()
			.NotEqual(0)
			.WithMessage("Amount must be non-zero.");
		RuleFor(x => x.Date)
			.NotEmpty()
			.LessThanOrEqualTo(DateTime.UtcNow)
			.WithMessage("Date must be prior to the current date.");
		RuleFor(x => x.Account)
			.NotNull()
			.WithMessage("Account is required.");
	}
}