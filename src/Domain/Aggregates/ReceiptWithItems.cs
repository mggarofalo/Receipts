using Domain.Core;

namespace Domain.Aggregates;

public class ReceiptWithItems
{
	public required Receipt Receipt { get; set; }
	public required List<ReceiptItem> Items { get; set; }
	public required List<Adjustment> Adjustments { get; set; }

	public Money Subtotal => Items.Aggregate(Money.Zero, (sum, item) => sum + item.TotalAmount);
	public Money AdjustmentTotal => Adjustments.Aggregate(Money.Zero, (sum, adj) => sum + adj.Amount);
	public Money ExpectedTotal => Subtotal + Receipt.TaxAmount + AdjustmentTotal;

	public const string TaxRateUnreasonable = "Tax rate ({0:P1}) is outside the typical 0–25% range";
	public const string AdjustmentRateUnreasonable = "Adjustment total ({0:P1} of subtotal) exceeds 10%";

	public List<ValidationWarning> GetWarnings()
	{
		List<ValidationWarning> warnings = [];

		if (Subtotal.Amount > 0)
		{
			decimal taxRate = Receipt.TaxAmount.Amount / Subtotal.Amount;
			if (taxRate < 0 || taxRate > 0.25m)
			{
				warnings.Add(new ValidationWarning(
					nameof(Receipt.TaxAmount),
					string.Format(TaxRateUnreasonable, taxRate),
					ValidationWarningSeverity.Warning));
			}

			decimal adjustmentRate = Math.Abs(AdjustmentTotal.Amount) / Subtotal.Amount;
			if (adjustmentRate > 0.10m)
			{
				warnings.Add(new ValidationWarning(
					nameof(AdjustmentTotal),
					string.Format(AdjustmentRateUnreasonable, adjustmentRate),
					ValidationWarningSeverity.Warning));
			}
		}

		return warnings;
	}
}
