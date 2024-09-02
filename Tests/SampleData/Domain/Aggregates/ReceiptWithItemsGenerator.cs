using Domain.Aggregates;
using Domain.Core;
using SampleData.Domain.Core;

namespace SampleData.Domain.Aggregates;

public static class ReceiptWithItemsGenerator
{
	public static ReceiptWithItems Generate()
	{
		Receipt receipt = ReceiptGenerator.Generate();
		List<ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(5, receipt.Id);

		return new ReceiptWithItems()
		{
			Receipt = receipt,
			Items = receiptItems
		};
	}

	public static List<ReceiptWithItems> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}
