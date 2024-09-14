using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Mapping;

public class ReceiptItemMappingProfileTests
{
	private readonly IMapper _mapper;

	public ReceiptItemMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptItemMappingProfile>();
		});

		// configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptItemToReceiptItemEntity()
	{
		// Arrange
		ReceiptItem original = ReceiptItemGenerator.Generate();

		// Act
		ReceiptItemEntity mapped = _mapper.MapToReceiptItemEntity(original, Guid.NewGuid());
		ReceiptItem reverseMapped = _mapper.Map<ReceiptItem>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptItemEntityToReceiptItem()
	{
		// Arrange
		ReceiptItemEntity original = ReceiptItemEntityGenerator.Generate();

		// Act
		ReceiptItem mapped = _mapper.Map<ReceiptItem>(original);
		ReceiptItemEntity reverseMapped = _mapper.MapToReceiptItemEntity(mapped, original.ReceiptId);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}
