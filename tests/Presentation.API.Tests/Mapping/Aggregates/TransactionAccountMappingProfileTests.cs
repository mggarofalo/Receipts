using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using API.Mapping.Core;
using API.Mapping.Aggregates;
using Microsoft.Extensions.Logging.Abstractions;

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
		}, NullLoggerFactory.Instance);

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapTransactionAccountToTransactionAccountVM()
	{
		// Arrange
		TransactionAccount expected = TransactionAccountGenerator.Generate();

		// Act
		TransactionAccountVM mapped = _mapper.Map<TransactionAccountVM>(expected);
		TransactionAccount actual = _mapper.Map<TransactionAccount>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ShouldMapTransactionAccountVMToTransactionAccount()
	{
		// Arrange
		TransactionAccountVM expected = TransactionAccountVMGenerator.Generate();

		// Act
		TransactionAccount mapped = _mapper.Map<TransactionAccount>(expected);
		TransactionAccountVM actual = _mapper.Map<TransactionAccountVM>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}
}