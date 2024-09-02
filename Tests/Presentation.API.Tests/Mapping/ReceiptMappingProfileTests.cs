using API.Mapping.Core;
using AutoMapper;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Mapping;

public class ReceiptMappingProfileTests
{
	private readonly IMapper _mapper;

	public ReceiptMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<ReceiptMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapReceiptToReceiptVM()
	{
		Guid receiptId = Guid.NewGuid();

		Receipt receipt = new(
			receiptId,
			"Location",
			new DateOnly(2024, 1, 1),
			new Money(100),
			"Description"
		);

		ReceiptVM receiptVM = _mapper.Map<ReceiptVM>(receipt);

		Assert.Equal(receipt.Id, receiptVM.Id);
		Assert.Equal(receipt.Location, receiptVM.Location);
		Assert.Equal(receipt.Date, receiptVM.Date);
		Assert.Equal(receipt.TaxAmount.Amount, receiptVM.TaxAmount);
	}

	[Fact]
	public void ShouldMapReceiptVMToReceipt()
	{
		Guid receiptId = Guid.NewGuid();
		ReceiptVM receiptVM = new()
		{
			Id = receiptId,
			Location = "Location",
			Date = new DateOnly(2024, 1, 1),
			TaxAmount = 100
		};

		Receipt receipt = _mapper.Map<Receipt>(receiptVM);

		Assert.Equal(receiptVM.Id, receipt.Id);
		Assert.Equal(receiptVM.Location, receipt.Location);
		Assert.Equal(receiptVM.Date, receipt.Date);
		Assert.Equal(receiptVM.TaxAmount, receipt.TaxAmount.Amount);
	}
}