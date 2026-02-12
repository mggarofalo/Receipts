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
}
