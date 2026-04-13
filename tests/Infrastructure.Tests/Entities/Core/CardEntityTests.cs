using Infrastructure.Entities.Core;

namespace Infrastructure.Tests.Entities.Core;

public class CardEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesCardEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string accountCode = "ACC001";
		string name = "Test Account";
		bool isActive = true;

		// Act
		CardEntity account = new()
		{
			Id = id,
			CardCode = accountCode,
			Name = name,
			IsActive = isActive
		};

		// Assert
		Assert.Equal(id, account.Id);
		Assert.Equal(accountCode, account.CardCode);
		Assert.Equal(name, account.Name);
		Assert.Equal(isActive, account.IsActive);
	}
}
