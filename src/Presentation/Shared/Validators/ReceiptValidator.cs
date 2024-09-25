using FluentValidation;
using Shared.ViewModels.Core;

namespace Shared.Validators;

public class ReceiptValidator : AbstractValidator<ReceiptVM>
{
	public const string DescriptionMustNotExceed256Characters = "Description must not exceed 256 characters.";
	public const string LocationIsRequired = "Location is required.";
	public const string LocationMustNotExceed200Characters = "Location must not exceed 200 characters.";
	public const string DateMustBePriorToCurrentDate = "Date must be prior to the current date";

	public ReceiptValidator()
	{
		RuleFor(x => x.Description)
			.MaximumLength(256)
			.WithMessage(DescriptionMustNotExceed256Characters);

		RuleFor(x => x.Location)
			.NotEmpty()
			.WithMessage(LocationIsRequired);

		RuleFor(x => x.Location)
			.MaximumLength(200)
			.WithMessage(LocationMustNotExceed200Characters);

		RuleFor(x => x.Date)
			.Must(date => date.HasValue && date.Value.ToDateTime(TimeOnly.MinValue) <= DateTime.Today)
			.WithMessage(DateMustBePriorToCurrentDate);
	}
}