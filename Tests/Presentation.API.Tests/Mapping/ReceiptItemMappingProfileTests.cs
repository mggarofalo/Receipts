using API.Mapping.Core;
using AutoMapper;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Mapping;

public class ReceiptItemMappingProfileTests
{
	private readonly IMapper _mapper;

	public ReceiptItemMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptItemMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptItemToReceiptItemVM()
	{
		ReceiptItem receiptItem = new(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"123456",
			"Test description",
			1,
			new Money(100),
			new Money(100),
			"Test category",
			"Test subcategory"
		);

		ReceiptItemVM receiptItemVM = _mapper.Map<ReceiptItemVM>(receiptItem);

		Assert.Equal(receiptItem.Id, receiptItemVM.Id);
		Assert.Equal(receiptItem.ReceiptId, receiptItemVM.ReceiptId);
		Assert.Equal(receiptItem.Description, receiptItemVM.Description);
		Assert.Equal(receiptItem.Quantity, receiptItemVM.Quantity);
		Assert.Equal(receiptItem.UnitPrice.Amount, receiptItemVM.UnitPrice);
		Assert.Equal(receiptItem.TotalAmount.Amount, receiptItemVM.TotalAmount);
		Assert.Equal(receiptItem.Category, receiptItemVM.Category);
		Assert.Equal(receiptItem.Subcategory, receiptItemVM.Subcategory);
	}

	[Fact]
	public void ShouldMapAccountVMToAccount()
	{
		ReceiptItemVM receiptItemVM = new()
		{
			Id = Guid.NewGuid(),
			ReceiptId = Guid.NewGuid(),
			ReceiptItemCode = "123456",
			Description = "Test description",
			Quantity = 1,
			UnitPrice = 100,
			TotalAmount = 100,
			Category = "Test category",
			Subcategory = "Test subcategory"
		};

		ReceiptItem receiptItem = _mapper.Map<ReceiptItem>(receiptItemVM);

		Assert.Equal(receiptItemVM.Id, receiptItem.Id);
		Assert.Equal(receiptItemVM.ReceiptId, receiptItem.ReceiptId);
		Assert.Equal(receiptItemVM.ReceiptItemCode, receiptItem.ReceiptItemCode);
		Assert.Equal(receiptItemVM.Description, receiptItem.Description);
		Assert.Equal(receiptItemVM.Quantity, receiptItem.Quantity);
		Assert.Equal(receiptItemVM.UnitPrice, receiptItem.UnitPrice.Amount);
		Assert.Equal(receiptItemVM.TotalAmount, receiptItem.TotalAmount.Amount);
		Assert.Equal(receiptItemVM.Category, receiptItem.Category);
		Assert.Equal(receiptItemVM.Subcategory, receiptItem.Subcategory);

	}
}