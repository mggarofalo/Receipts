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
		Card account = new(id, accountCode, name, isActive);

		// Assert
		Assert.Equal(id, account.Id);
		Assert.Equal(accountCode, account.CardCode);
		Assert.Equal(name, account.Name);
		Assert.Equal(isActive, account.IsActive);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesAccountWithEmptyId()
	{
		// Arrange
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act
		Card account = new(Guid.Empty, accountCode, name, isActive);

		// Assert
		Assert.Equal(Guid.Empty, account.Id);
		Assert.Equal(accountCode, account.CardCode);
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
		Card account = new(id, accountCode, name);

		// Assert
		Assert.True(account.IsActive);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidCardCode_ThrowsArgumentException(string invalidCardCode)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Account";
		bool isActive = true;

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Card(id, invalidCardCode, name, isActive));
		Assert.StartsWith(Card.CardCodeCannotBeEmpty, exception.Message);
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
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Card(id, accountCode, invalidName, isActive));
		Assert.StartsWith(Card.NameCannotBeEmpty, exception.Message);
	}
}
