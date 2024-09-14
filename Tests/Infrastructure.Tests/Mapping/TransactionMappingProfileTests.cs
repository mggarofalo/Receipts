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
		Transaction original = TransactionGenerator.Generate();

		// Act
		TransactionEntity mapped = _mapper.MapToTransactionEntity(original, Guid.NewGuid(), Guid.NewGuid());
		Transaction reverseMapped = _mapper.Map<Transaction>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapTransactionEntityToTransaction()
	{
		// Arrange
		TransactionEntity original = TransactionEntityGenerator.Generate();

		// Act
		Transaction mapped = _mapper.Map<Transaction>(original);
		TransactionEntity reverseMapped = _mapper.MapToTransactionEntity(mapped, original.ReceiptId, original.AccountId);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}
