using API.Generated.Dtos;
using FluentValidation;

namespace API.Validators;

public class BulkPushYnabTransactionsRequestValidator : AbstractValidator<BulkPushYnabTransactionsRequest>
{
	public const int MaxReceiptIds = 100;
	public const string ReceiptIdsMustNotBeEmpty = "At least one receipt ID is required.";
	public const string ReceiptIdsTooMany = "Cannot push more than 100 receipts at once.";
	public const string ReceiptIdMustNotBeEmpty = "Each receipt ID must not be empty.";

	public BulkPushYnabTransactionsRequestValidator()
	{
		RuleFor(x => x.ReceiptIds)
			.NotEmpty()
			.WithMessage(ReceiptIdsMustNotBeEmpty);

		RuleFor(x => x.ReceiptIds)
			.Must(ids => ids is null || ids.Count <= MaxReceiptIds)
			.WithMessage(ReceiptIdsTooMany);

		RuleForEach(x => x.ReceiptIds)
			.NotEmpty()
			.WithMessage(ReceiptIdMustNotBeEmpty);
	}
}
