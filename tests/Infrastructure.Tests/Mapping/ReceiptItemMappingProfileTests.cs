using AutoMapper;
using Domain.Core;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
using Microsoft.Extensions.Logging.Abstractions;
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
		}, NullLoggerFactory.Instance);

		// configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptItemToReceiptItemEntity()
	{
		// Arrange
		ReceiptItem expected = ReceiptItemGenerator.Generate();

		// Act
		ReceiptItemEntity mapped = _mapper.MapToReceiptItemEntity(expected, Guid.NewGuid());
		ReceiptItem actual = _mapper.Map<ReceiptItem>(mapped);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void ShouldMapReceiptItemEntityToReceiptItem()
	{
		// Arrange
		ReceiptItemEntity expected = ReceiptItemEntityGenerator.Generate();

		// Act
		ReceiptItem mapped = _mapper.Map<ReceiptItem>(expected);
		ReceiptItemEntity actual = _mapper.MapToReceiptItemEntity(mapped, expected.ReceiptId);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void ShouldFailToMapReceiptItemToReceiptItemEntityWithStandardMappingCall()
	{
		// Arrange
		ReceiptItem expected = ReceiptItemGenerator.Generate();

		// Act & Assert
		Assert.Throws<AutoMapperMappingException>(() => _mapper.Map<ReceiptItemEntity>(expected));
	}
}
