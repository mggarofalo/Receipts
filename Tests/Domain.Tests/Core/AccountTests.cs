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

		// Act
		Account account = new(id, accountCode, name);

		// Assert
		Assert.Equal(id, account.Id);
		Assert.Equal(accountCode, account.AccountCode);
		Assert.Equal(name, account.Name);
		Assert.True(account.IsActive);
	}

	[Fact]
	public void Constructor_NullId_CreatesAccountWithNullId()
	{
		// Arrange
		string accountCode = "ACC001";
		string name = "Test Account";

		// Act
		Account account = new(null, accountCode, name);

		// Assert
		Assert.Null(account.Id);
		Assert.Equal(accountCode, account.AccountCode);
		Assert.Equal(name, account.Name);
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

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Account(id, invalidAccountCode, name));
		Assert.Equal("Account code cannot be empty (Parameter 'accountCode')", exception.Message);
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

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Account(id, accountCode, invalidName));
		Assert.Equal("Name cannot be empty (Parameter 'name')", exception.Message);
	}

	[Fact]
	public void Constructor_InactiveAccount_CreatesInactiveAccount()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = false;

		// Act
		Account account = new(id, accountCode, name, isActive);

		// Assert
		Assert.False(account.IsActive);
	}
}