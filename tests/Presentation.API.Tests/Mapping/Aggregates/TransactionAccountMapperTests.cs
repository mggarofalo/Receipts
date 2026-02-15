using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Common;
using Domain;
using Domain.Aggregates;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Aggregates;

public class TransactionAccountMapperTests
{
	private readonly TransactionAccountMapper _mapper = new();

	[Fact]
	public void ToResponse_MapsTransactionAndAccount()
	{
		// Arrange
		Guid transactionId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		Transaction transaction = new(
			transactionId,
			new Money(250.75m, Currency.USD),
			new DateOnly(2025, 4, 15)
		);

		Account account = new(
			accountId,
			"CHECKING-001",
			"Primary Checking",
			true
		);

		TransactionAccount aggregate = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Act
		TransactionAccountResponse actual = _mapper.ToResponse(aggregate);

		// Assert — Transaction
		Assert.Equal(transactionId, actual.Transaction.Id);
		Assert.Equal((double)250.75m, actual.Transaction.Amount);
		Assert.Equal(new DateOnly(2025, 4, 15), actual.Transaction.Date);

		// Assert — Account
		Assert.Equal(accountId, actual.Account.Id);
		Assert.Equal("CHECKING-001", actual.Account.AccountCode);
		Assert.Equal("Primary Checking", actual.Account.Name);
		Assert.True(actual.Account.IsActive);
	}

	[Fact]
	public void ToResponse_MapsInactiveAccount()
	{
		// Arrange
		Guid transactionId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		Transaction transaction = new(
			transactionId,
			new Money(75.00m, Currency.USD),
			new DateOnly(2025, 6, 1)
		);

		Account account = new(
			accountId,
			"SAVINGS-002",
			"Old Savings",
			false
		);

		TransactionAccount aggregate = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Act
		TransactionAccountResponse actual = _mapper.ToResponse(aggregate);

		// Assert
		Assert.False(actual.Account.IsActive);
		Assert.Equal("SAVINGS-002", actual.Account.AccountCode);
	}

	[Fact]
	public void ToResponse_MapsNegativeTransactionAmount()
	{
		// Arrange
		Guid transactionId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		Transaction transaction = new(
			transactionId,
			new Money(-500.50m, Currency.USD),
			new DateOnly(2025, 8, 22)
		);

		Account account = new(
			accountId,
			"CC-001",
			"Credit Card",
			true
		);

		TransactionAccount aggregate = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Act
		TransactionAccountResponse actual = _mapper.ToResponse(aggregate);

		// Assert
		Assert.Equal((double)(-500.50m), actual.Transaction.Amount);
	}
}
