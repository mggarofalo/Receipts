using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Core;

public class TransactionVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTransactionVM()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		decimal amount = 100.0m;
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		// Act
		TransactionVM transactionVM = new()
		{
			Id = id,
			Amount = amount,
			Date = date
		};

		// Assert
		Assert.Equal(id, transactionVM.Id);
		Assert.Equal(amount, transactionVM.Amount);
		Assert.Equal(date, transactionVM.Date);
	}

	[Fact]
	public void Constructor_NullId_CreatesTransactionVMWithNullId()
	{
		// Arrange
		decimal amount = 100.0m;
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		// Act
		TransactionVM transactionVM = new()
		{
			Amount = amount,
			Date = date
		};

		// Assert
		Assert.Null(transactionVM.Id);
	}
}
