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
		Receipt expected = ReceiptGenerator.Generate();

		// Act
		ReceiptEntity mapped = _mapper.Map<ReceiptEntity>(expected);
		Receipt actual = _mapper.Map<Receipt>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ShouldMapReceiptEntityToReceipt()
	{
		// Arrange
		ReceiptEntity expected = ReceiptEntityGenerator.Generate();

		// Act
		Receipt mapped = _mapper.Map<Receipt>(expected);
		ReceiptEntity actual = _mapper.Map<ReceiptEntity>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}
}
