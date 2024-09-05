using FluentValidation;
using Shared.ViewModels.Core;

namespace Shared.Validators;

public class TransactionValidator : AbstractValidator<TransactionVM>
{
	public TransactionValidator()
	{
		RuleFor(x => x.Amount)
			.NotEmpty()
			.NotEqual(0)
			.WithMessage("Amount must be non-zero.");

		RuleFor(x => x.Date.ToDateTime(TimeOnly.MinValue))
			.NotEmpty()
			.LessThanOrEqualTo(DateTime.Today)
			.WithMessage("Date must be prior to the current date.");

		RuleFor(x => x.AccountId)
			.NotEmpty()
			.WithMessage("Account is required.");
	}
}