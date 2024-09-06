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

		_mapper = configuration.CreateMapper();
		_mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}

	[Fact]
	public void ShouldMapReceiptToReceiptVM()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();

		// Act
		ReceiptVM receiptVM = _mapper.Map<ReceiptVM>(receipt);
		Receipt reverseMapped = _mapper.Map<Receipt>(receiptVM);

		// Assert
		Assert.Equal(receipt, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptVMToReceipt()
	{
		// Arrange
		ReceiptVM receiptVM = ReceiptVMGenerator.Generate();

		// Act
		Receipt receipt = _mapper.Map<Receipt>(receiptVM);
		ReceiptVM reverseMapped = _mapper.Map<ReceiptVM>(receipt);

		// Assert
		Assert.Equal(receiptVM, reverseMapped);
	}
}