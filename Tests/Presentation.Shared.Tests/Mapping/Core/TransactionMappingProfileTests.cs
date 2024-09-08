using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.Mapping.Core;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.Mapping.Core;

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
	public void ShouldMapTransactionToTransactionVM()
	{
		// Arrange
		Transaction original = TransactionGenerator.Generate();

		// Act
		TransactionVM mapped = _mapper.Map<TransactionVM>(original);
		Transaction reverseMapped = _mapper.Map<Transaction>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapTransactionVMToTransaction()
	{
		// Arrange
		TransactionVM original = TransactionVMGenerator.Generate();

		// Act
		Transaction mapped = _mapper.Map<Transaction>(original);
		TransactionVM reverseMapped = _mapper.Map<TransactionVM>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}