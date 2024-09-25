using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using API.Mapping.Core;
using API.Mapping.Aggregates;

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

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptWithItemsToReceiptWithItemsVM()
	{
		// Arrange
		ReceiptWithItems expected = ReceiptWithItemsGenerator.Generate();

		// Act
		ReceiptWithItemsVM mapped = _mapper.Map<ReceiptWithItemsVM>(expected);
		ReceiptWithItems actual = _mapper.Map<ReceiptWithItems>(mapped);

		// Assert
		Assert.Equal(expected.Items, actual.Items);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ShouldMapReceiptWithItemsVMToReceiptWithItems()
	{
		// Arrange
		ReceiptWithItemsVM expected = ReceiptWithItemsVMGenerator.Generate();
		Guid receiptId = expected.Receipt!.Id!.Value;

		// Act
		ReceiptWithItems mapped = _mapper.Map<ReceiptWithItems>(expected);
		ReceiptWithItemsVM actual = _mapper.Map<ReceiptWithItemsVM>(mapped);

		// Assert
		Assert.Equal(expected.Items, actual.Items);
		Assert.Equal(expected, actual);
	}
}