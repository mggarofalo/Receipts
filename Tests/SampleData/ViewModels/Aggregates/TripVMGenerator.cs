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

	public static List<TripVM> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}
