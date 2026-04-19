using Domain.Core;

namespace Domain.Tests.Core;

public class AccountTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesAccount()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Account";
		bool isActive = true;

		// Act
		Account account = new(id, name, isActive);

		// Assert
		Assert.Equal(id, account.Id);
		Assert.Equal(name, account.Name);
		Assert.Equal(isActive, account.IsActive);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesAccountWithEmptyId()
	{
		// Arrange
		string name = "Test Account";
		bool isActive = true;

		// Act
		Account account = new(Guid.Empty, name, isActive);

		// Assert
		Assert.Equal(Guid.Empty, account.Id);
		Assert.Equal(name, account.Name);
		Assert.Equal(isActive, account.IsActive);
	}

	[Fact]
	public void Constructor_DefaultIsActive_CreatesActiveAccount()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Account";

		// Act
		Account account = new(id, name);

		// Assert
		Assert.True(account.IsActive);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		bool isActive = true;

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Account(id, invalidName, isActive));
		Assert.StartsWith(Account.NameCannotBeEmpty, exception.Message);
	}
}
