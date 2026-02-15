using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class CreateReceiptRequestValidator : AbstractValidator<CreateReceiptRequest>
{
	public const string DescriptionMustNotExceed256Characters = "Description must not exceed 256 characters.";
	public const string LocationMustNotExceed200Characters = "Location must not exceed 200 characters.";
	public const string DateMustBePriorToCurrentDate = "Date must be prior to the current date";

	public CreateReceiptRequestValidator()
	{
		RuleFor(x => x.Description)
			.MaximumLength(256)
			.WithMessage(DescriptionMustNotExceed256Characters);

		RuleFor(x => x.Location)
			.MaximumLength(200)
			.WithMessage(LocationMustNotExceed200Characters);

		RuleFor(x => x.Date)
			.Must(date => date.ToDateTime(TimeOnly.MinValue) <= DateTime.Today)
			.WithMessage(DateMustBePriorToCurrentDate);
	}
}
