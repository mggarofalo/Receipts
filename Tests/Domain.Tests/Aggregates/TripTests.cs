using Domain.Aggregates;
using Domain.Core;

namespace Domain.Tests.Aggregates;

public class TripTests
{
	[Fact]
	public void Trip_ShouldHaveRequiredProperties()
	{
		// Arrange
		Receipt receipt = new(Guid.NewGuid(), "Test Receipt", DateOnly.FromDateTime(DateTime.Now), new Money(100), "Test Description");

		List<ReceiptItem> items =
		[
			new(Guid.NewGuid(), Guid.NewGuid(), "Test Item 1", "Test Description 1", 100, new Money(100), new Money(100), "Test Category 1", "Test Subcategory 1"),
			new(Guid.NewGuid(), Guid.NewGuid(), "Test Item 2", "Test Description 2", 200, new Money(200), new Money(200), "Test Category 2", "Test Subcategory 2")
		];

		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = items
		};

		List<TransactionAccount> transactions =
		[
			new()
			{
				Transaction = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100), DateOnly.FromDateTime(DateTime.Now)),
				Account = new(Guid.NewGuid(), "Test Account 1", "Test Description 1")
			},
			new()
			{
				Transaction = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(200), DateOnly.FromDateTime(DateTime.Now)),
				Account = new(Guid.NewGuid(), "Test Account 2", "Test Description 2")
			}
		];

		// Act
		Trip trip = new()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};

		// Assert
		Assert.NotNull(trip.Receipt);
		Assert.NotNull(trip.Transactions);
		Assert.Equal(2, trip.Transactions.Count);
	}
}
