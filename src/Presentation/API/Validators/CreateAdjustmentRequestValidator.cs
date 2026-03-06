using API.Generated.Dtos;
using Common;
using FluentValidation;

namespace API.Validators;

public class CreateAdjustmentRequestValidator : AbstractValidator<CreateAdjustmentRequest>
{
	public const string TypeMustNotBeEmpty = "Type must not be empty.";
	public const string TypeMustBeValid = "Type must be a valid adjustment type.";
	public const string AmountMustBeNonZero = "Amount must be non-zero.";
	public const string DescriptionRequiredForOtherType = "Description is required when type is 'other'.";

	private static readonly string[] ValidTypeNames = Enum.GetNames<AdjustmentType>();

	public CreateAdjustmentRequestValidator()
	{
		RuleFor(x => x.Type)
			.NotEmpty()
			.WithMessage(TypeMustNotBeEmpty)
			.Must(type => ValidTypeNames.Contains(type, StringComparer.OrdinalIgnoreCase))
			.WithMessage(TypeMustBeValid);

		RuleFor(x => x.Amount)
			.NotEqual(0)
			.WithMessage(AmountMustBeNonZero);

		RuleFor(x => x.Description)
			.NotEmpty()
			.WithMessage(DescriptionRequiredForOtherType)
			.When(x => string.Equals(x.Type, nameof(AdjustmentType.Other), StringComparison.OrdinalIgnoreCase));
	}
}
