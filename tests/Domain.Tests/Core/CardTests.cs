using Domain.Core;

namespace Domain.Tests.Core;

public class CardTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesAccount()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		Guid accountId = Guid.NewGuid();
		bool isActive = true;

		// Act
		Card card = new(id, accountCode, name, accountId, isActive);

		// Assert
		Assert.Equal(id, card.Id);
		Assert.Equal(accountCode, card.CardCode);
		Assert.Equal(name, card.Name);
		Assert.Equal(accountId, card.AccountId);
		Assert.Equal(isActive, card.IsActive);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesCardWithEmptyId()
	{
		// Arrange
		string accountCode = "ACC001";
		string name = "Test Account";
		Guid accountId = Guid.NewGuid();
		bool isActive = true;

		// Act
		Card card = new(Guid.Empty, accountCode, name, accountId, isActive);

		// Assert
		Assert.Equal(Guid.Empty, card.Id);
		Assert.Equal(accountCode, card.CardCode);
		Assert.Equal(name, card.Name);
		Assert.Equal(accountId, card.AccountId);
		Assert.Equal(isActive, card.IsActive);
	}

	[Fact]
	public void Constructor_DefaultIsActive_CreatesActiveCard()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		Guid accountId = Guid.NewGuid();

		// Act
		Card card = new(id, accountCode, name, accountId);

		// Assert
		Assert.True(card.IsActive);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidCardCode_ThrowsArgumentException(string? invalidCardCode)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Account";
		Guid accountId = Guid.NewGuid();
		bool isActive = true;

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Card(id, invalidCardCode!, name, accountId, isActive));
		Assert.StartsWith(Card.CardCodeCannotBeEmpty, exception.Message);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidName_ThrowsArgumentException(string? invalidName)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		Guid accountId = Guid.NewGuid();
		bool isActive = true;

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Card(id, accountCode, invalidName!, accountId, isActive));
		Assert.StartsWith(Card.NameCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_EmptyAccountId_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Card(id, accountCode, name, Guid.Empty, isActive));
		Assert.StartsWith(Card.AccountIdCannotBeEmpty, exception.Message);
	}
}
