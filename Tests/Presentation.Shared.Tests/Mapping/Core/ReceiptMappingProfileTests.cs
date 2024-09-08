using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.Mapping.Core;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.Mapping.Core;

public class ReceiptMappingProfileTests
{
	private readonly IMapper _mapper;

	public ReceiptMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptMappingProfile>();
		});

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptToReceiptVM()
	{
		// Arrange
		Receipt original = ReceiptGenerator.Generate();

		// Act
		ReceiptVM mapped = _mapper.Map<ReceiptVM>(original);
		Receipt reverseMapped = _mapper.Map<Receipt>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptVMToReceipt()
	{
		// Arrange
		ReceiptVM original = ReceiptVMGenerator.Generate();

		// Act
		Receipt mapped = _mapper.Map<Receipt>(original);
		ReceiptVM reverseMapped = _mapper.Map<ReceiptVM>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}