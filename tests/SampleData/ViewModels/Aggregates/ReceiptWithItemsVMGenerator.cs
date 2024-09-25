using SampleData.ViewModels.Core;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Aggregates;

public static class ReceiptWithItemsVMGenerator
{
	public static ReceiptWithItemsVM Generate()
	{
		ReceiptVM receipt = ReceiptVMGenerator.Generate();
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(5);

		return new ReceiptWithItemsVM()
		{
			Receipt = receipt,
			Items = receiptItems
		};
	}
}
