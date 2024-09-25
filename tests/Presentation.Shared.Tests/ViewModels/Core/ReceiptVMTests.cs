using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Core;

public class ReceiptVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptVM()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Description";
		string location = "Test Location";
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);
		decimal taxAmount = 10.0m;

		// Act
		ReceiptVM receiptVM = new()
		{
			Id = id,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};

		// Assert
		Assert.Equal(id, receiptVM.Id);
		Assert.Equal(description, receiptVM.Description);
		Assert.Equal(location, receiptVM.Location);
		Assert.Equal(date, receiptVM.Date);
		Assert.Equal(taxAmount, receiptVM.TaxAmount);
	}

	[Fact]
	public void Constructor_NullId_CreatesReceiptVMWithNullId()
	{
		// Arrange
		string description = "Test Description";
		string location = "Test Location";
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);
		decimal taxAmount = 10.0m;

		// Act
		ReceiptVM receiptVM = new()
		{
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};

		// Assert
		Assert.Null(receiptVM.Id);
	}

	[Fact]
	public void Equals_SameReceiptVM_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Description";
		string location = "Test Location";
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);
		decimal taxAmount = 10.0m;

		ReceiptVM receiptVM1 = new()
		{
			Id = id,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};
		ReceiptVM receiptVM2 = new()
		{
			Id = id,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};

		// Act & Assert
		Assert.Equal(receiptVM1, receiptVM2);
	}

	[Fact]
	public void Equals_DifferentReceiptVM_ReturnsFalse()
	{
		// Arrange
		ReceiptVM receiptVM1 = ReceiptVMGenerator.Generate();
		ReceiptVM receiptVM2 = ReceiptVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptVM1, receiptVM2);
	}

	[Fact]
	public void Equals_NullReceiptVM_ReturnsFalse()
	{
		// Arrange
		ReceiptVM receiptVM = ReceiptVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		ReceiptVM receiptVM = ReceiptVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		ReceiptVM receiptVM = ReceiptVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptVM.Equals("not a receiptVM"));
	}

	[Fact]
	public void GetHashCode_SameReceiptVM_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Description";
		string location = "Test Location";
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);
		decimal taxAmount = 10.0m;

		ReceiptVM receiptVM1 = new()
		{
			Id = id,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};
		ReceiptVM receiptVM2 = new()
		{
			Id = id,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};

		// Act & Assert
		Assert.Equal(receiptVM1.GetHashCode(), receiptVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_NullId_ReturnsSameHashCode()
	{
		// Arrange
		ReceiptVM receiptVM1 = new()
		{
			Description = "Test Description",
			Location = "Test Location",
			Date = DateOnly.FromDateTime(DateTime.Now),
			TaxAmount = 10.0m
		};
		ReceiptVM receiptVM2 = new()
		{
			Description = "Test Description",
			Location = "Test Location",
			Date = DateOnly.FromDateTime(DateTime.Now),
			TaxAmount = 10.0m
		};

		// Act & Assert
		Assert.Equal(receiptVM1.GetHashCode(), receiptVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptVM_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		string description = "Test Description";
		string location = "Test Location";
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);
		decimal taxAmount = 10.0m;

		ReceiptVM receiptVM1 = new()
		{
			Id = id1,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};
		ReceiptVM receiptVM2 = new()
		{
			Id = id2,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};

		// Act & Assert
		Assert.NotEqual(receiptVM1.GetHashCode(), receiptVM2.GetHashCode());
	}

	[Fact]
	public void OperatorEquals_SameReceiptVM_ReturnsTrue()
	{
		// Arrange
		ReceiptVM receiptVM1 = ReceiptVMGenerator.Generate();
		ReceiptVM receiptVM2 = new()
		{
			Id = receiptVM1.Id,
			Description = receiptVM1.Description,
			Location = receiptVM1.Location,
			Date = receiptVM1.Date,
			TaxAmount = receiptVM1.TaxAmount
		};

		// Act
		bool result = receiptVM1 == receiptVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_DifferentReceiptVM_ReturnsFalse()
	{
		// Arrange
		ReceiptVM receiptVM1 = ReceiptVMGenerator.Generate();
		ReceiptVM receiptVM2 = ReceiptVMGenerator.Generate();

		// Act
		bool result = receiptVM1 == receiptVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_SameReceiptVM_ReturnsFalse()
	{
		// Arrange
		ReceiptVM receiptVM1 = ReceiptVMGenerator.Generate();
		ReceiptVM receiptVM2 = new()
		{
			Id = receiptVM1.Id,
			Description = receiptVM1.Description,
			Location = receiptVM1.Location,
			Date = receiptVM1.Date,
			TaxAmount = receiptVM1.TaxAmount
		};

		// Act
		bool result = receiptVM1 != receiptVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_DifferentReceiptVM_ReturnsTrue()
	{
		// Arrange
		ReceiptVM receiptVM1 = ReceiptVMGenerator.Generate();
		ReceiptVM receiptVM2 = ReceiptVMGenerator.Generate();

		// Act
		bool result = receiptVM1 != receiptVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_NullReceiptVM_ReturnsFalse()
	{
		// Arrange
		ReceiptVM receiptVM = ReceiptVMGenerator.Generate();

		// Act
		bool result = receiptVM == null;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_NullReceiptVM_ReturnsTrue()
	{
		// Arrange
		ReceiptVM receiptVM = ReceiptVMGenerator.Generate();

		// Act
		bool result = receiptVM != null;

		// Assert
		Assert.True(result);
	}
}
