using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class UpdateReceiptItemRequestValidator : AbstractValidator<UpdateReceiptItemRequest>
{
	public const string UnitPriceMustBePositive = "Unit price must be positive.";

	public UpdateReceiptItemRequestValidator()
	{
		RuleFor(x => x.UnitPrice)
			.GreaterThan(0)
			.WithMessage(UnitPriceMustBePositive);
	}
}
