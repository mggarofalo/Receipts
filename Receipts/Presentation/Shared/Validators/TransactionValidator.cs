using FluentValidation;
using Shared.ViewModels.Core;

namespace Shared.Validators;

public class TransactionValidator : AbstractValidator<TransactionVM>
{
	public const string AmountMustBeNonZero = "Amount must be non-zero.";
	public const string DateMustBePriorToCurrentDate = "Date must be prior to the current date";

	public TransactionValidator()
	{
		RuleFor(x => x.Amount)
			.NotEmpty()
			.NotEqual(0)
			.WithMessage(AmountMustBeNonZero);

		RuleFor(x => x.Date.ToDateTime(TimeOnly.MinValue))
			.NotEmpty()
			.LessThanOrEqualTo(DateTime.Today)
			.WithMessage(DateMustBePriorToCurrentDate);
	}
}