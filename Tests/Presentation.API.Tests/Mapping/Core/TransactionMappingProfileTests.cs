using API.Mapping.Core;
using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class TransactionMappingProfileTests
{
	private readonly IMapper _mapper;

	public TransactionMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TransactionMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}

	[Fact]
	public void ShouldMapTransactionToTransactionVM()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();

		// Act
		TransactionVM transactionVM = _mapper.Map<TransactionVM>(transaction);
		Transaction reverseMapped = _mapper.Map<Transaction>(transactionVM);

		// Assert
		Assert.Equal(transaction, reverseMapped);
	}

	[Fact]
	public void ShouldMapTransactionVMToTransaction()
	{
		// Arrange
		TransactionVM transactionVM = TransactionVMGenerator.Generate();

		// Act
		Transaction transaction = _mapper.Map<Transaction>(transactionVM);
		TransactionVM reverseMapped = _mapper.Map<TransactionVM>(transaction);

		// Assert
		Assert.Equal(transactionVM, reverseMapped);
	}
}