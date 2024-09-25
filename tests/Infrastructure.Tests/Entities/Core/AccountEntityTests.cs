using Infrastructure.Entities.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Entities.Core;

public class AccountEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesAccountEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act
		AccountEntity account = new()
		{
			Id = id,
			AccountCode = accountCode,
			Name = name,
			IsActive = isActive
		};

		// Assert
		Assert.Equal(id, account.Id);
		Assert.Equal(accountCode, account.AccountCode);
		Assert.Equal(name, account.Name);
		Assert.Equal(isActive, account.IsActive);
	}

	[Fact]
	public void Equals_SameAccountEntity_ReturnsTrue()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = new()
		{
			Id = account1.Id,
			AccountCode = account1.AccountCode,
			Name = account1.Name,
			IsActive = account1.IsActive
		};

		// Act & Assert
		Assert.Equal(account1, account2);
	}

	[Fact]
	public void Equals_DifferentAccountEntity_ReturnsFalse()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = AccountEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(account1, account2);
	}

	[Fact]
	public void Equals_NullAccountEntity_ReturnsFalse()
	{
		// Arrange
		AccountEntity account = AccountEntityGenerator.Generate();

		// Act & Assert
		Assert.False(account.Equals(null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		AccountEntity account = AccountEntityGenerator.Generate();

		// Act & Assert
		Assert.False(account.Equals("not an account entity"));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		AccountEntity account = AccountEntityGenerator.Generate();

		// Act & Assert
		Assert.False(account.Equals((object?)null));
	}

	[Fact]
	public void GetHashCode_SameAccountEntity_ReturnsSameHashCode()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = new()
		{
			Id = account1.Id,
			AccountCode = account1.AccountCode,
			Name = account1.Name,
			IsActive = account1.IsActive
		};

		// Act & Assert
		Assert.Equal(account1.GetHashCode(), account2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentAccountEntity_ReturnsDifferentHashCode()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = AccountEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(account1.GetHashCode(), account2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameAccountEntity_ReturnsTrue()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = account1;

		// Act
		bool result = account1 == account2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentAccountEntity_ReturnsFalse()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = AccountEntityGenerator.Generate();

		// Act
		bool result = account1 == account2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameAccountEntity_ReturnsFalse()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = account1;

		// Act
		bool result = account1 != account2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentAccountEntity_ReturnsTrue()
	{
		// Arrange
		AccountEntity account1 = AccountEntityGenerator.Generate();
		AccountEntity account2 = AccountEntityGenerator.Generate();

		// Act
		bool result = account1 != account2;

		// Assert
		Assert.True(result);
	}
}
