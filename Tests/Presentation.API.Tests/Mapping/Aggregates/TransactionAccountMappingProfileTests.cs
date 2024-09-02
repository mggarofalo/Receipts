using API.Mapping.Aggregates;
using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using API.Mapping.Core;

namespace Presentation.API.Tests.Mapping.Aggregates;

public class TransactionAccountMappingProfileTests
{
	private readonly IMapper _mapper;

	public TransactionAccountMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TransactionAccountMappingProfile>();
			cfg.AddProfile<TransactionMappingProfile>();
			cfg.AddProfile<AccountMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapTransactionAccountToTransactionAccountVMAndBackWithoutLosingData()
	{
		// Arrange
		TransactionAccount original = TransactionAccountGenerator.Generate();

		// Act
		TransactionAccountVM mapped = _mapper.Map<TransactionAccountVM>(original);
		TransactionAccount reverseMapped = _mapper.Map<TransactionAccount>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapTransactionAccountVMToTransactionAccountAndBackWithoutLosingData()
	{
		// Arrange
		TransactionAccountVM original = TransactionAccountVMGenerator.Generate();

		// Act
		TransactionAccount mapped = _mapper.Map<TransactionAccount>(original);
		TransactionAccountVM reverseMapped = _mapper.Map<TransactionAccountVM>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}