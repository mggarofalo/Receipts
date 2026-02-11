using Domain.Aggregates;
using FluentAssertions;
using SampleData.Domain.Aggregates;

namespace Domain.Tests.Aggregates;

public class TripTests
{
	[Fact]
	public void Trip_ShouldHaveRequiredProperties()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();
		List<TransactionAccount> transactions = TransactionAccountGenerator.GenerateList(2);

		// Act
		Trip trip = new()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};

		// Assert
		Assert.NotNull(trip.Receipt);
		trip.Receipt.Should().BeSameAs(receiptWithItems);
		Assert.NotNull(trip.Transactions);
		trip.Transactions.Should().BeSameAs(transactions);
	}
}
