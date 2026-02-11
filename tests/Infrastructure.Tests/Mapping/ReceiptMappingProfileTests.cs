using AutoMapper;
using Domain.Core;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
using Microsoft.Extensions.Logging.Abstractions;
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
		}, NullLoggerFactory.Instance);

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
		actual.Should().BeEquivalentTo(expected);
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
		actual.Should().BeEquivalentTo(expected);
	}
}
