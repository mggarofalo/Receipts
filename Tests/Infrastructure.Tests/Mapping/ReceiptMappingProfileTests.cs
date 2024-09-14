using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Mapping;

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
	public void ShouldMapReceiptToReceiptEntity()
	{
		// Arrange
		Receipt original = ReceiptGenerator.Generate();

		// Act
		ReceiptEntity mapped = _mapper.Map<ReceiptEntity>(original);
		Receipt reverseMapped = _mapper.Map<Receipt>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptEntityToReceipt()
	{
		// Arrange
		ReceiptEntity original = ReceiptEntityGenerator.Generate();

		// Act
		Receipt mapped = _mapper.Map<Receipt>(original);
		ReceiptEntity reverseMapped = _mapper.Map<ReceiptEntity>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}
