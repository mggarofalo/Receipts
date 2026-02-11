using API.Mapping.Core;
using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace Presentation.API.Tests.Mapping.Core;

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
	public void ShouldMapReceiptToReceiptVM()
	{
		// Arrange
		Receipt expected = ReceiptGenerator.Generate();

		// Act
		ReceiptVM mapped = _mapper.Map<ReceiptVM>(expected);
		Receipt actual = _mapper.Map<Receipt>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ShouldMapReceiptVMToReceipt()
	{
		// Arrange
		ReceiptVM expected = ReceiptVMGenerator.Generate();

		// Act
		Receipt mapped = _mapper.Map<Receipt>(expected);
		ReceiptVM actual = _mapper.Map<ReceiptVM>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}
}