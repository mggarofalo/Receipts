using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using Shared.Mapping.Aggregates;
using Shared.Mapping.Core;

namespace Presentation.Shared.Tests.Mapping.Aggregates;

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

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptWithItemsToReceiptWithItemsVM()
	{
		// Arrange
		ReceiptWithItems original = ReceiptWithItemsGenerator.Generate();

		// Act
		ReceiptWithItemsVM mapped = _mapper.Map<ReceiptWithItemsVM>(original);
		ReceiptWithItems reverseMapped = _mapper.Map<ReceiptWithItems>(mapped);

		// Assert
		Assert.Equal(original.Items, reverseMapped.Items);
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptWithItemsVMToReceiptWithItems()
	{
		// Arrange
		ReceiptWithItemsVM original = ReceiptWithItemsVMGenerator.Generate();
		Guid receiptId = original.Receipt.Id!.Value;

		// Act
		ReceiptWithItems mapped = _mapper.Map<ReceiptWithItems>(original);
		ReceiptWithItemsVM reverseMapped = _mapper.Map<ReceiptWithItemsVM>(mapped);

		// Assert
		Assert.Equal(original.Items, reverseMapped.Items);
		Assert.Equal(original, reverseMapped);
	}
}