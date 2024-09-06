using API.Mapping.Aggregates;
using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using API.Mapping.Core;

namespace Presentation.API.Tests.Mapping.Aggregates;

public class ReceiptWithItemsMappingProfileTests
{
	private readonly IMapper _mapper;

	public ReceiptWithItemsMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptWithItemsMappingProfile>();
			cfg.AddProfile<ReceiptMappingProfile>();
			cfg.AddProfile<ReceiptItemMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}

	[Fact]
	public void ShouldMapReceiptWithItemsToReceiptWithItemsVM()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();

		// Act
		ReceiptWithItemsVM receiptWithItemsVM = _mapper.Map<ReceiptWithItemsVM>(receiptWithItems);
		ReceiptWithItems reverseMapped = _mapper.Map<ReceiptWithItems>(receiptWithItemsVM);

		// Assert
		Assert.Equal(receiptWithItems, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptWithItemsVMToReceiptWithItems()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act
		ReceiptWithItems receiptWithItems = _mapper.Map<ReceiptWithItems>(receiptWithItemsVM);
		ReceiptWithItemsVM reverseMapped = _mapper.Map<ReceiptWithItemsVM>(receiptWithItems);

		// Assert
		Assert.Equal(receiptWithItemsVM, reverseMapped);
	}
}