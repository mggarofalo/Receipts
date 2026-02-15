using API.Generated.Dtos;
using API.Mapping.Core;
using Common;
using Domain;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class TransactionMapperTests
{
	private readonly TransactionMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		CreateTransactionRequest request = new()
		{
			Amount = 150.75,
			Date = new DateOnly(2025, 3, 20)
		};

		// Act
		Transaction actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal(150.75m, actual.Amount.Amount);
		Assert.Equal(Currency.USD, actual.Amount.Currency);
		Assert.Equal(new DateOnly(2025, 3, 20), actual.Date);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_MapsNegativeAmount()
	{
		// Arrange
		CreateTransactionRequest request = new()
		{
			Amount = -42.50,
			Date = new DateOnly(2025, 2, 14)
		};

		// Act
		Transaction actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal(-42.50m, actual.Amount.Amount);
		Assert.Equal(Currency.USD, actual.Amount.Currency);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateTransactionRequest request = new()
		{
			Id = expectedId,
			Amount = 275.00,
			Date = new DateOnly(2025, 7, 4)
		};

		// Act
		Transaction actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal(275.00m, actual.Amount.Amount);
		Assert.Equal(Currency.USD, actual.Amount.Currency);
		Assert.Equal(new DateOnly(2025, 7, 4), actual.Date);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Transaction transaction = new(
			expectedId,
			new Money(99.95m, Currency.USD),
			new DateOnly(2025, 5, 15)
		);

		// Act
		TransactionResponse actual = _mapper.ToResponse(transaction);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal((double)99.95m, actual.Amount);
		Assert.Equal(new DateOnly(2025, 5, 15), actual.Date);
	}

	[Fact]
	public void ToResponse_FlattensMoneyAmountToDouble()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Transaction transaction = new(
			expectedId,
			new Money(123.4567m, Currency.USD),
			new DateOnly(2025, 1, 1)
		);

		// Act
		TransactionResponse actual = _mapper.ToResponse(transaction);

		// Assert
		Assert.Equal((double)123.4567m, actual.Amount);
	}

	[Fact]
	public void ToResponse_MapsDateOnly()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		DateOnly expectedDate = new(2025, 12, 31);
		Transaction transaction = new(
			expectedId,
			new Money(50.00m, Currency.USD),
			expectedDate
		);

		// Act
		TransactionResponse actual = _mapper.ToResponse(transaction);

		// Assert
		Assert.Equal(expectedDate, actual.Date);
	}
}
