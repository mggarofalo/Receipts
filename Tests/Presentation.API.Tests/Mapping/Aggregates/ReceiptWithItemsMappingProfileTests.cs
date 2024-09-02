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
	}

	[Fact]
	public void ShouldMapReceiptWithItemsToReceiptWithItemsVMAndBackWithoutLosingData()
	{
		// Arrange
		ReceiptWithItems original = ReceiptWithItemsGenerator.Generate();

		// Act
		ReceiptWithItemsVM mapped = _mapper.Map<ReceiptWithItemsVM>(original);
		ReceiptWithItems reverseMapped = _mapper.Map<ReceiptWithItems>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptWithItemsVMToReceiptWithItemsAndBackWithoutLosingData()
	{
		// Arrange
		ReceiptWithItemsVM original = ReceiptWithItemsVMGenerator.Generate();

		// Act
		ReceiptWithItems mapped = _mapper.Map<ReceiptWithItems>(original);
		ReceiptWithItemsVM reverseMapped = _mapper.Map<ReceiptWithItemsVM>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}