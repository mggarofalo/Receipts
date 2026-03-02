using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class CreateReceiptItemRequestValidator : AbstractValidator<CreateReceiptItemRequest>
{
	public const string UnitPriceMustBePositive = "Unit price must be positive.";

	public CreateReceiptItemRequestValidator()
	{
		RuleFor(x => x.UnitPrice)
			.GreaterThan(0)
			.WithMessage(UnitPriceMustBePositive);
	}
}
