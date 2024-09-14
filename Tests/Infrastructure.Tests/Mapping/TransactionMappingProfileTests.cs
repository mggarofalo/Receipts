using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Mapping;

public class TransactionMappingProfileTests
{
	private readonly IMapper _mapper;

	public TransactionMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TransactionMappingProfile>();
		});

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapTransactionToTransactionEntity()
	{
		// Arrange
		Transaction expected = TransactionGenerator.Generate();

		// Act
		TransactionEntity mapped = _mapper.MapToTransactionEntity(expected, Guid.NewGuid(), Guid.NewGuid());
		Transaction actual = _mapper.Map<Transaction>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ShouldMapTransactionEntityToTransaction()
	{
		// Arrange
		TransactionEntity expected = TransactionEntityGenerator.Generate();

		// Act
		Transaction mapped = _mapper.Map<Transaction>(expected);
		TransactionEntity actual = _mapper.MapToTransactionEntity(mapped, expected.ReceiptId, expected.AccountId);

		// Assert
		Assert.Equal(expected, actual);
	}
}
