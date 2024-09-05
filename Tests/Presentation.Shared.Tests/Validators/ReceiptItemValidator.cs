using FluentValidation;
using Shared.ViewModels.Core;

namespace Shared.Validators;

public class ReceiptItemValidator : AbstractValidator<ReceiptItemVM>
{
	public ReceiptItemValidator()
	{
		RuleFor(x => x.ReceiptItemCode)
			.NotEmpty()
			.WithMessage("Receipt item code is required.");
		RuleFor(x => x.Description)
			.NotEmpty()
			.WithMessage("Description is required.");
		RuleFor(x => x.Category)
			.NotEmpty()
			.WithMessage("Category is required.");
		RuleFor(x => x.Subcategory)
			.NotEmpty()
			.WithMessage("Subcategory is required.");
	}
}