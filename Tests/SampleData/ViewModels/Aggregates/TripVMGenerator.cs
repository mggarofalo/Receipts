using Shared.ViewModels.Aggregates;

namespace SampleData.ViewModels.Aggregates;

public static class TripVMGenerator
{
	public static TripVM Generate()
	{
		ReceiptWithItemsVM receiptWithItems = ReceiptWithItemsVMGenerator.Generate();
		List<TransactionAccountVM> transactions = TransactionAccountVMGenerator.GenerateList(2);

		return new TripVM()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};
	}
}
