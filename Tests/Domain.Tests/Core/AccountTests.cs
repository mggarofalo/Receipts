using Domain.Core;
using SampleData.Domain.Core;

namespace Domain.Tests.Core;

public class AccountTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesAccount()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act
		Account account = new(id, accountCode, name, isActive);

		// Assert
		Assert.Equal(id, account.Id);
		Assert.Equal(accountCode, account.AccountCode);
		Assert.Equal(name, account.Name);
		Assert.Equal(isActive, account.IsActive);
	}

	[Fact]
	public void Constructor_NullId_CreatesAccountWithNullId()
	{
		// Arrange
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act
		Account account = new(null, accountCode, name, isActive);

		// Assert
		Assert.Null(account.Id);
		Assert.Equal(accountCode, account.AccountCode);
		Assert.Equal(name, account.Name);
		Assert.Equal(isActive, account.IsActive);
	}

	[Fact]
	public void Constructor_DefaultIsActive_CreatesActiveAccount()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";

		// Act
		Account account = new(id, accountCode, name);

		// Assert
		Assert.True(account.IsActive);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidAccountCode_ThrowsArgumentException(string invalidAccountCode)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Account";
		bool isActive = true;

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Account(id, invalidAccountCode, name, isActive));
		Assert.StartsWith(Account.AccountCodeCannotBeEmpty, exception.Message);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		bool isActive = true;

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Account(id, accountCode, invalidName, isActive));
		Assert.StartsWith(Account.NameCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Equals_SameAccount_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		Account account1 = new(id, accountCode, name, isActive);
		Account account2 = new(id, accountCode, name, isActive);

		// Act & Assert
		Assert.Equal(account1, account2);
	}

	[Fact]
	public void Equals_DifferentAccount_ReturnsFalse()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		Account account1 = new(id1, accountCode, name, isActive);
		Account account2 = new(id2, accountCode, name, isActive);

		// Act & Assert
		Assert.NotEqual(account1, account2);
	}

	[Fact]
	public void Equals_DifferentAccountCode_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode1 = "ACC001";
		string accountCode2 = "ACC002";
		string name = "Test Account";
		bool isActive = true;

		Account account1 = new(id, accountCode1, name, isActive);
		Account account2 = new(id, accountCode2, name, isActive);

		// Act & Assert
		Assert.NotEqual(account1, account2);
	}

	[Fact]
	public void Equals_DifferentName_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name1 = "Test Account 1";
		string name2 = "Test Account 2";
		bool isActive = true;

		Account account1 = new(id, accountCode, name1, isActive);
		Account account2 = new(id, accountCode, name2, isActive);

		// Act & Assert
		Assert.NotEqual(account1, account2);
	}

	[Fact]
	public void Equals_DifferentIsActive_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive1 = true;
		bool isActive2 = false;

		Account account1 = new(id, accountCode, name, isActive1);
		Account account2 = new(id, accountCode, name, isActive2);

		// Act & Assert
		Assert.NotEqual(account1, account2);
	}

	[Fact]
	public void Equals_NullAccount_ReturnsFalse()
	{
		// Arrange
		Account account = AccountGenerator.Generate();

		// Act & Assert
		Assert.False(account.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		Account account = AccountGenerator.Generate();

		// Act & Assert
		Assert.False(account.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		Account account = AccountGenerator.Generate();

		// Act & Assert
		Assert.False(account.Equals("not an account"));
	}

	[Fact]
	public void GetHashCode_SameAccount_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		Account account1 = new(id, accountCode, name, isActive);
		Account account2 = new(id, accountCode, name, isActive);

		// Act & Assert
		Assert.Equal(account1.GetHashCode(), account2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentAccount_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		Account account1 = new(id1, accountCode, name, isActive);
		Account account2 = new(id2, accountCode, name, isActive);

		// Act & Assert
		Assert.NotEqual(account1.GetHashCode(), account2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameAccount_ReturnsTrue()
	{
		// Arrange
		Account account1 = AccountGenerator.Generate();
		Account account2 = account1;

		// Act
		bool result = account1 == account2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentAccounts_ReturnsFalse()
	{
		// Arrange
		Account account1 = AccountGenerator.Generate();
		Account account2 = AccountGenerator.Generate();

		// Act
		bool result = account1 == account2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameAccount_ReturnsFalse()
	{
		// Arrange
		Account account1 = AccountGenerator.Generate();
		Account account2 = account1;

		// Act
		bool result = account1 != account2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentAccounts_ReturnsTrue()
	{
		// Arrange
		Account account1 = AccountGenerator.Generate();
		Account account2 = AccountGenerator.Generate();

		// Act
		bool result = account1 != account2;

		// Assert
		Assert.True(result);
	}
}