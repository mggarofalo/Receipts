using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.Mapping.Core;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.Mapping.Core;

public class ReceiptItemMappingProfileTests
{
	private readonly IMapper _mapper;

	public ReceiptItemMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptItemMappingProfile>();
		});

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptItemToReceiptItemVM()
	{
		// Arrange
		ReceiptItem original = ReceiptItemGenerator.Generate();

		// Act
		ReceiptItemVM mapped = _mapper.Map<ReceiptItemVM>(original);
		ReceiptItem reverseMapped = _mapper.Map<ReceiptItem>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapReceiptItemVMToReceiptItem()
	{
		// Arrange
		ReceiptItemVM original = ReceiptItemVMGenerator.Generate();
		Guid receiptId = Guid.NewGuid();

		// Act
		ReceiptItem mapped = _mapper.Map<ReceiptItem>(original);
		ReceiptItemVM reverseMapped = _mapper.Map<ReceiptItemVM>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}