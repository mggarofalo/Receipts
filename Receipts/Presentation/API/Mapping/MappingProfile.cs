using AutoMapper;
using Domain;
using Domain.Core;

namespace API.Mapping;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		MapAccount();
		MapReceipt();
		MapTransaction();
		MapReceiptItem();
	}

	private void MapAccount()
	{
		CreateMap<Account, Shared.ViewModels.AccountVM>().ReverseMap();
	}


	private void MapReceipt()
	{
		CreateMap<Receipt, Shared.ViewModels.ReceiptVM>()
					.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount))
					.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<Shared.ViewModels.ReceiptVM, Receipt>()
			.ConstructUsing(src => Receipt.Create(
				src.Location,
				src.Date,
				new Money(src.TaxAmount, "USD"),
				new Money(src.TotalAmount, "USD"),
				src.Description
			));
	}


	private void MapTransaction()
	{
		CreateMap<Transaction, Shared.ViewModels.TransactionVM>()
					.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount));

		CreateMap<Shared.ViewModels.TransactionVM, Transaction>()
			.ConstructUsing(src => Transaction.Create(
				src.ReceiptId,
				src.AccountId,
				new Money(src.Amount, "USD"),
				src.Date
			));
	}


	private void MapReceiptItem()
	{
		CreateMap<ReceiptItem, Shared.ViewModels.ReceiptItemVM>()
					.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
					.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<Shared.ViewModels.ReceiptItemVM, ReceiptItem>()
			.ConstructUsing(src => ReceiptItem.Create(
				src.ReceiptItemCode,
				src.Description,
				src.Quantity,
				new Money(src.UnitPrice, "USD"),
				src.Category,
				src.Subcategory
			));
	}
}
