using Common;
using Domain;
using Domain.Core;

namespace SampleData.Domain.Core;

public static class ReceiptItemGenerator
{
	public static ReceiptItem Generate(PricingMode pricingMode = PricingMode.Quantity)
	{
		return new ReceiptItem(
			Guid.NewGuid(),
			"ITEMCODE",
			"Test Item",
			1,
			new Money(5),
			new Money(5),
			"Test Category",
			"Test Subcategory",
			pricingMode
		);
	}

	public static List<ReceiptItem> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}
