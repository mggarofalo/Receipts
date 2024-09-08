using Domain.Aggregates;
using Domain.Core;
using SampleData.Domain.Core;

namespace SampleData.Domain.Aggregates;

public static class ReceiptWithItemsGenerator
{
	public static ReceiptWithItems Generate()
	{
		Receipt receipt = ReceiptGenerator.Generate();
		List<ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(5);

		return new ReceiptWithItems()
		{
			Receipt = receipt,
			Items = receiptItems
		};
	}
}
