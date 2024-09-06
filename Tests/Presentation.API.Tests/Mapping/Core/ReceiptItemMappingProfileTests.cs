using API.Mapping.Core;
using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class ReceiptItemMappingProfileTests
{
	private readonly IMapper _mapper;

	public ReceiptItemMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptItemMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}

	[Fact]
	public void ShouldMapReceiptItemToReceiptItemVM()
	{
		// Arrange
		ReceiptItem receiptItem = ReceiptItemGenerator.Generate();

		// Act
		ReceiptItemVM receiptItemVM = _mapper.Map<ReceiptItemVM>(receiptItem);
		ReceiptItem reverseMapped = _mapper.Map<ReceiptItem>(receiptItemVM);

		// Assert
		Assert.Equal(receiptItem, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptItemVMToReceiptItem()
	{
		// Arrange
		ReceiptItemVM receiptItemVM = ReceiptItemVMGenerator.Generate();

		// Act
		ReceiptItem receiptItem = _mapper.Map<ReceiptItem>(receiptItemVM);
		ReceiptItemVM reverseMapped = _mapper.Map<ReceiptItemVM>(receiptItem);

		// Assert
		Assert.Equal(receiptItemVM, reverseMapped);
	}
}