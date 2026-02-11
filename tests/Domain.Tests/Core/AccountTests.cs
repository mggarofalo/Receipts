using Domain.Core;

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
}
