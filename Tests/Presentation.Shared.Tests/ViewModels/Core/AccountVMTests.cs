using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Core;

public class AccountVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesAccountVM()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act
		AccountVM accountVM = new()
		{
			Id = id,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};

		// Assert
		Assert.Equal(id, accountVM.Id);
		Assert.Equal(accountCode, accountVM.AccountCode);
		Assert.Equal(name, accountVM.Name);
		Assert.True(accountVM.IsActive);
	}

	[Fact]
	public void Constructor_NullId_CreatesAccountVMWithNullId()
	{
		// Arrange
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act
		AccountVM accountVM = new()
		{
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};

		// Assert
		Assert.Null(accountVM.Id);
	}

	[Fact]
	public void Equals_SameAccountVM_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		AccountVM accountVM1 = new()
		{
			Id = id,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};
		AccountVM accountVM2 = new()
		{
			Id = id,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};

		// Act & Assert
		Assert.Equal(accountVM1, accountVM2);
	}

	[Fact]
	public void Equals_DifferentAccountVM_ReturnsFalse()
	{
		// Arrange
		AccountVM accountVM1 = AccountVMGenerator.Generate();
		AccountVM accountVM2 = AccountVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(accountVM1, accountVM2);
	}

	[Fact]
	public void Equals_NullAccountVM_ReturnsFalse()
	{
		// Arrange
		AccountVM accountVM = AccountVMGenerator.Generate();

		// Act & Assert
		Assert.False(accountVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		AccountVM accountVM = AccountVMGenerator.Generate();

		// Act & Assert
		Assert.False(accountVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		AccountVM accountVM = AccountVMGenerator.Generate();

		// Act & Assert
		Assert.False(accountVM.Equals("not an accountVM"));
	}

	[Fact]
	public void GetHashCode_SameAccountVM_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		AccountVM accountVM1 = new()
		{
			Id = id,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};
		AccountVM accountVM2 = new()
		{
			Id = id,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};

		// Act & Assert
		Assert.Equal(accountVM1.GetHashCode(), accountVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_NullId_ReturnsSameHashCode()
	{
		// Arrange
		AccountVM accountVM1 = new()
		{
			AccountCode = "ACC001",
			Name = "Test Account",
			IsActive = true
		};
		AccountVM accountVM2 = new()
		{
			AccountCode = "ACC001",
			Name = "Test Account",
			IsActive = true
		};

		// Act & Assert
		Assert.Equal(accountVM1.GetHashCode(), accountVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentAccountVM_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		AccountVM accountVM1 = new()
		{
			Id = id1,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};
		AccountVM accountVM2 = new()
		{
			Id = id2,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};

		// Act & Assert
		Assert.NotEqual(accountVM1.GetHashCode(), accountVM2.GetHashCode());
	}

	[Fact]
	public void OperatorEquals_SameAccountVM_ReturnsTrue()
	{
		// Arrange
		AccountVM accountVM1 = AccountVMGenerator.Generate();
		AccountVM accountVM2 = new()
		{
			Id = accountVM1.Id,
			AccountCode = accountVM1.AccountCode,
			Name = accountVM1.Name,
			IsActive = accountVM1.IsActive
		};

		// Act
		bool result = accountVM1 == accountVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_DifferentAccountVM_ReturnsFalse()
	{
		// Arrange
		AccountVM accountVM1 = AccountVMGenerator.Generate();
		AccountVM accountVM2 = AccountVMGenerator.Generate();

		// Act
		bool result = accountVM1 == accountVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_SameAccountVM_ReturnsFalse()
	{
		// Arrange
		AccountVM accountVM1 = AccountVMGenerator.Generate();
		AccountVM accountVM2 = new()
		{
			Id = accountVM1.Id,
			AccountCode = accountVM1.AccountCode,
			Name = accountVM1.Name,
			IsActive = accountVM1.IsActive
		};

		// Act
		bool result = accountVM1 != accountVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_DifferentAccountVM_ReturnsTrue()
	{
		// Arrange
		AccountVM accountVM1 = AccountVMGenerator.Generate();
		AccountVM accountVM2 = AccountVMGenerator.Generate();

		// Act
		bool result = accountVM1 != accountVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_NullAccountVM_ReturnsFalse()
	{
		// Arrange
		AccountVM accountVM = AccountVMGenerator.Generate();

		// Act
		bool result = accountVM == null;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_NullAccountVM_ReturnsTrue()
	{
		// Arrange
		AccountVM accountVM = AccountVMGenerator.Generate();

		// Act
		bool result = accountVM != null;

		// Assert
		Assert.True(result);
	}
}
