using SampleData.ViewModels.Core;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Aggregates;

public static class ReceiptWithItemsVMGenerator
{
	public static ReceiptWithItemsVM Generate()
	{
		ReceiptVM receipt = ReceiptVMGenerator.Generate();
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(5, receipt.Id);

		return new ReceiptWithItemsVM()
		{
			Receipt = receipt,
			Items = receiptItems
		};
	}

	public static List<ReceiptWithItemsVM> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}
