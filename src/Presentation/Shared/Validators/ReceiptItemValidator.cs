using FluentValidation;
using Shared.ViewModels.Core;

namespace Shared.Validators;

public class ReceiptItemValidator : AbstractValidator<ReceiptItemVM>
{
	public const string ReceiptItemCodeIsRequired = "Receipt item code is required.";
	public const string DescriptionIsRequired = "Description is required.";
	public const string CategoryIsRequired = "Category is required.";
	public const string SubcategoryIsRequired = "Subcategory is required.";

	public ReceiptItemValidator()
	{
		RuleFor(x => x.ReceiptItemCode)
			.NotEmpty()
			.WithMessage(ReceiptItemCodeIsRequired);

		RuleFor(x => x.Description)
			.NotEmpty()
			.WithMessage(DescriptionIsRequired);

		RuleFor(x => x.Category)
			.NotEmpty()
			.WithMessage(CategoryIsRequired);

		RuleFor(x => x.Subcategory)
			.NotEmpty()
			.WithMessage(SubcategoryIsRequired);
	}
}