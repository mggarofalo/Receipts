using SampleData.ViewModels.Aggregates;
using Shared.ViewModels.Aggregates;

namespace Presentation.Shared.Tests.ViewModels.Aggregates;

public class TripVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTripVM()
	{
		// Arrange
		ReceiptWithItemsVM receipt = ReceiptWithItemsVMGenerator.Generate();
		List<TransactionAccountVM> transactions =
		[
			TransactionAccountVMGenerator.Generate(),
			TransactionAccountVMGenerator.Generate()
		];

		// Act
		TripVM tripVM = new()
		{
			Receipt = receipt,
			Transactions = transactions
		};

		// Assert
		Assert.Equal(receipt, tripVM.Receipt);
		Assert.Equal(transactions, tripVM.Transactions);
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
	public void Equals_DifferentTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM1 = new()
		{
			Receipt = ReceiptWithItemsVMGenerator.Generate(),
			Transactions = [TransactionAccountVMGenerator.Generate()]
		};

		TripVM tripVM2 = new()
		{
			Receipt = ReceiptWithItemsVMGenerator.Generate(),
			Transactions = [TransactionAccountVMGenerator.Generate()]
		};

		// Act & Assert
		Assert.NotEqual(tripVM1, tripVM2);
	}

	[Fact]
	public void Equals_NullTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = new()
		{
			Receipt = ReceiptWithItemsVMGenerator.Generate(),
			Transactions = [TransactionAccountVMGenerator.Generate()]
		};

		// Act & Assert
		Assert.False(tripVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = new()
		{
			Receipt = ReceiptWithItemsVMGenerator.Generate(),
			Transactions = [TransactionAccountVMGenerator.Generate()]
		};

		// Act & Assert
		Assert.False(tripVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = new()
		{
			Receipt = ReceiptWithItemsVMGenerator.Generate(),
			Transactions = [TransactionAccountVMGenerator.Generate()]
		};

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

		// Act & Assert
		Assert.Equal(tripVM1.GetHashCode(), tripVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTripVM_ReturnsDifferentHashCode()
	{
		// Arrange
		TripVM tripVM1 = new()
		{
			Receipt = ReceiptWithItemsVMGenerator.Generate(),
			Transactions = [TransactionAccountVMGenerator.Generate()]
		};

		TripVM tripVM2 = new()
		{
			Receipt = ReceiptWithItemsVMGenerator.Generate(),
			Transactions = [TransactionAccountVMGenerator.Generate()]
		};

		// Act & Assert
		Assert.NotEqual(tripVM1.GetHashCode(), tripVM2.GetHashCode());
	}

	[Fact]
	public void OperatorEquals_SameTripVM_ReturnsTrue()
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
		bool result = tripVM1 == tripVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_DifferentTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM1 = TripVMGenerator.Generate();
		TripVM tripVM2 = TripVMGenerator.Generate();

		// Act
		bool result = tripVM1 == tripVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_SameTripVM_ReturnsFalse()
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
		bool result = tripVM1 != tripVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_DifferentTripVM_ReturnsTrue()
	{
		// Arrange
		TripVM tripVM1 = TripVMGenerator.Generate();
		TripVM tripVM2 = TripVMGenerator.Generate();

		// Act
		bool result = tripVM1 != tripVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_NullTripVM_ReturnsFalse()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act
		bool result = tripVM == null;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_NullTripVM_ReturnsTrue()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act
		bool result = tripVM != null;

		// Assert
		Assert.True(result);
	}
}