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
		_mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}

	[Fact]
	public void ShouldMapTransactionAccountToTransactionAccountVM()
	{
		// Arrange
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();

		// Act
		TransactionAccountVM transactionAccountVM = _mapper.Map<TransactionAccountVM>(transactionAccount);
		TransactionAccount reverseMapped = _mapper.Map<TransactionAccount>(transactionAccountVM);

		// Assert
		Assert.Equal(transactionAccount, reverseMapped);
	}

	[Fact]
	public void ShouldMapTransactionAccountVMToTransactionAccount()
	{
		// Arrange
		TransactionAccountVM transactionAccountVM = TransactionAccountVMGenerator.Generate();

		// Act
		TransactionAccount transactionAccount = _mapper.Map<TransactionAccount>(transactionAccountVM);
		TransactionAccountVM reverseMapped = _mapper.Map<TransactionAccountVM>(transactionAccount);

		// Assert
		Assert.Equal(transactionAccountVM, reverseMapped);
	}
}