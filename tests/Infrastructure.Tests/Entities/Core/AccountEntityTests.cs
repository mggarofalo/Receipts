using Infrastructure.Entities.Core;

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
}
