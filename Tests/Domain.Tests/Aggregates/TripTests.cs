using Domain.Aggregates;
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
		Assert.Equal(receiptWithItems, trip.Receipt);
		Assert.NotNull(trip.Transactions);
		Assert.Equal(transactions, trip.Transactions);
	}

	[Fact]
	public void Equals_SameTrip_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();
		List<TransactionAccount> transactions = TransactionAccountGenerator.GenerateList(2);

		Trip trip1 = new()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};

		Trip trip2 = new()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};

		// Act & Assert
		Assert.Equal(trip1, trip2);
	}

	[Fact]
	public void Equals_DifferentTrip_ReturnsFalse()
	{
		// Arrange
		Trip trip1 = TripGenerator.Generate();
		Trip trip2 = TripGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(trip1, trip2);
	}

	[Fact]
	public void Equals_NullTrip_ReturnsFalse()
	{
		// Arrange
		Trip trip = TripGenerator.Generate();

		// Act & Assert
		Assert.False(trip.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		Trip trip = TripGenerator.Generate();

		// Act & Assert
		Assert.False(trip.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		Trip trip = TripGenerator.Generate();

		// Act & Assert
		Assert.False(trip.Equals("not a trip"));
	}

	[Fact]
	public void GetHashCode_SameTrip_ReturnsSameHashCode()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();
		List<TransactionAccount> transactions = TransactionAccountGenerator.GenerateList(2);

		Trip trip1 = new()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};

		Trip trip2 = new()
		{
			Receipt = receiptWithItems,
			Transactions = transactions
		};

		// Act & Assert
		Assert.Equal(trip1.GetHashCode(), trip2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTrip_ReturnsDifferentHashCode()
	{
		// Arrange
		Trip trip1 = TripGenerator.Generate();
		Trip trip2 = TripGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(trip1.GetHashCode(), trip2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameTrip_ReturnsTrue()
	{
		// Arrange
		Trip trip1 = TripGenerator.Generate();
		Trip trip2 = trip1;

		// Act
		bool result = trip1 == trip2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentTrip_ReturnsFalse()
	{
		// Arrange
		Trip trip1 = TripGenerator.Generate();
		Trip trip2 = TripGenerator.Generate();

		// Act
		bool result = trip1 == trip2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameTrip_ReturnsFalse()
	{
		// Arrange
		Trip trip1 = TripGenerator.Generate();
		Trip trip2 = trip1;

		// Act
		bool result = trip1 != trip2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentTrip_ReturnsTrue()
	{
		// Arrange
		Trip trip1 = TripGenerator.Generate();
		Trip trip2 = TripGenerator.Generate();

		// Act
		bool result = trip1 != trip2;

		// Assert
		Assert.True(result);
	}
}
