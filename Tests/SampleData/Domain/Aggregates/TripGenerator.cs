using Domain.Aggregates;

namespace SampleData.Domain.Aggregates;

public static class TripGenerator
{
	public static Trip Generate()
	{
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();
		List<TransactionAccount> transactions = TransactionAccountGenerator.GenerateList(2);

		return new Trip()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};
	}

	public static List<Trip> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}
