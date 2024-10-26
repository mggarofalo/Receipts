using SampleData.ViewModels.Aggregates;
using Shared.ViewModels.Aggregates;

namespace Presentation.Shared.Tests.ViewModels.Aggregates;

public class TripVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTripVM()
	{
		// Arrange
		ReceiptWithItemsVM expectedReceipt = ReceiptWithItemsVMGenerator.Generate();
		List<TransactionAccountVM> expectedTransactions =
		[
			TransactionAccountVMGenerator.Generate(),
			TransactionAccountVMGenerator.Generate()
		];

		// Act
		TripVM tripVM = new()
		{
			Receipt = expectedReceipt,
			Transactions = expectedTransactions
		};

		// Assert
		Assert.Equal(expectedReceipt, tripVM.Receipt);
		Assert.Equal(expectedTransactions, tripVM.Transactions);
		Assert.Equal(2, tripVM.Transactions.Count);
	}

	[Fact]
	public void Equals_SameTripVM_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receipt = ReceiptWithItemsVMGenerator.Generate();
		List<TransactionAccountVM> transactions =
		[
			TransactionAccountVMGenerator.Generate(),
			TransactionAccountVMGenerator.Generate()
		];

		TripVM tripVM1 = new()
		{
			Receipt = receipt,
			Transactions = transactions
		};

		TripVM tripVM2 = new()
		{
			Receipt = receipt,
			Transactions = transactions
		};

		// Act & Assert
		Assert.Equal(tripVM1, tripVM2);
	}

	[Fact]
	public void Equals_BothTripVMHaveNullTransactions_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receipt = ReceiptWithItemsVMGenerator.Generate();

		TripVM tripVM1 = new()
		{
			Receipt = receipt,
			Transactions = null
		};

		TripVM tripVM2 = new()
		{
			Receipt = receipt,
			Transactions = null
		};

		// Act & Assert
		Assert.Equal(tripVM1, tripVM2);
	}

	[Fact]
	public void Equals_OneTripVMHasNullTransactionsOtherHasEmptyTransactions_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receipt = ReceiptWithItemsVMGenerator.Generate();

		TripVM tripVM1 = new()
		{
			Receipt = receipt,
			Transactions = null
		};

		TripVM tripVM2 = new()
		{
			Receipt = receipt,
			Transactions = []
		};

		// Act & Assert
		Assert.Equal(tripVM1, tripVM2);
	}

	[Fact]
	public void Equals_DifferentTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM expected = TripVMGenerator.Generate();
		TripVM actual = TripVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(expected, actual);
	}

	[Fact]
	public void Equals_NullTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act & Assert
		Assert.False(tripVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act & Assert
		Assert.False(tripVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act & Assert
		Assert.False(tripVM.Equals("not a trip VM"));
	}

	[Fact]
	public void GetHashCode_SameTripVM_ReturnsSameHashCode()
	{
		// Arrange
		ReceiptWithItemsVM receipt = ReceiptWithItemsVMGenerator.Generate();
		List<TransactionAccountVM> transactions =
		[
			TransactionAccountVMGenerator.Generate(),
			TransactionAccountVMGenerator.Generate()
		];

		TripVM tripVM1 = new()
		{
			Receipt = receipt,
			Transactions = transactions
		};

		TripVM tripVM2 = new()
		{
			Receipt = receipt,
			Transactions = transactions
		};

		// Act
		int hashCode1 = tripVM1.GetHashCode();
		int hashCode2 = tripVM2.GetHashCode();

		// Assert
		Assert.Equal(hashCode1, hashCode2);
	}

	[Fact]
	public void GetHashCode_DifferentTripVM_ReturnsDifferentHashCode()
	{
		// Arrange
		TripVM tripVM1 = TripVMGenerator.Generate();
		TripVM tripVM2 = TripVMGenerator.Generate();

		// Act
		int hashCode1 = tripVM1.GetHashCode();
		int hashCode2 = tripVM2.GetHashCode();

		// Assert
		Assert.NotEqual(hashCode1, hashCode2);
	}

	[Fact]
	public void OperatorEquals_SameTripVM_ReturnsTrue()
	{
		// Arrange
		TripVM tripVM1 = TripVMGenerator.Generate();
		TripVM tripVM2 = new()
		{
			Receipt = tripVM1.Receipt,
			Transactions = tripVM1.Transactions
		};

		// Act
		bool actual = tripVM1 == tripVM2;

		// Assert
		Assert.True(actual);
	}

	[Fact]
	public void OperatorEquals_DifferentTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM1 = TripVMGenerator.Generate();
		TripVM tripVM2 = TripVMGenerator.Generate();

		// Act
		bool actual = tripVM1 == tripVM2;

		// Assert
		Assert.False(actual);
	}

	[Fact]
	public void OperatorNotEquals_SameTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM1 = TripVMGenerator.Generate();
		TripVM tripVM2 = new()
		{
			Receipt = tripVM1.Receipt,
			Transactions = tripVM1.Transactions
		};

		// Act
		bool actual = tripVM1 != tripVM2;

		// Assert
		Assert.False(actual);
	}

	[Fact]
	public void OperatorNotEquals_DifferentTripVM_ReturnsTrue()
	{
		// Arrange
		TripVM tripVM1 = TripVMGenerator.Generate();
		TripVM tripVM2 = TripVMGenerator.Generate();

		// Act
		bool actual = tripVM1 != tripVM2;

		// Assert
		Assert.True(actual);
	}

	[Fact]
	public void OperatorEquals_NullTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act
		bool actual = tripVM == null;

		// Assert
		Assert.False(actual);
	}

	[Fact]
	public void OperatorNotEquals_NullTripVM_ReturnsTrue()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act
		bool actual = tripVM != null;

		// Assert
		Assert.True(actual);
	}
}