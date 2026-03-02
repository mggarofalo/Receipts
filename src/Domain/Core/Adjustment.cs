using Common;

namespace Domain.Core;

public class Adjustment
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public AdjustmentType Type { get; set; }
	public Money Amount { get; set; }
	public string? Description { get; set; }

	public const string AmountMustBeNonZero = "Amount must be non-zero";
	public const string DescriptionRequiredForOtherType = "Description is required when adjustment type is Other";

	public Adjustment(Guid id, AdjustmentType type, Money amount, string? description = null)
	{
		if (amount.Amount == 0)
		{
			throw new ArgumentException(AmountMustBeNonZero, nameof(amount));
		}

		if (type == AdjustmentType.Other && string.IsNullOrWhiteSpace(description))
		{
			throw new ArgumentException(DescriptionRequiredForOtherType, nameof(description));
		}

		Id = id;
		Type = type;
		Amount = amount;
		Description = description;
	}
}
