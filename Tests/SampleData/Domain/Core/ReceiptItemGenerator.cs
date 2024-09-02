using Domain;
using Domain.Core;

namespace SampleData.Domain.Core;

public static class ReceiptItemGenerator
{
	public static ReceiptItem Generate(Guid? receiptId = null)
	{
		return new ReceiptItem(
			Guid.NewGuid(),
			receiptId ?? Guid.NewGuid(),
			"ITEMCODE",
			"Test Item",
			1,
			new Money(5),
			new Money(5),
			"Test Category",
			"Test Subcategory"
		);
	}

	public static List<ReceiptItem> GenerateList(int count, Guid? receiptId = null)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate(receiptId))
			.ToList();
	}
}
