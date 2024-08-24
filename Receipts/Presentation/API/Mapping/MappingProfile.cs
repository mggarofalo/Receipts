using AutoMapper;
using Domain;
using Domain.Core;
using Application.Commands.Receipt;
using Shared.ViewModels;

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
		CreateMap<Account, AccountVM>().ReverseMap();
	}

	private void MapReceipt()
	{
		CreateMap<Receipt, ReceiptVM>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<ReceiptVM, Receipt>()
			.ConstructUsing(src => Receipt.Create(
				src.Location,
				src.Date,
				new Money(src.TaxAmount, "USD"),
				new Money(src.TotalAmount, "USD"),
				src.Description
			));

		CreateMap<ReceiptVM, CreateReceiptCommand>()
			.ConstructUsing(src => new CreateReceiptCommand(
				src.Location,
				src.Date,
				src.TaxAmount,
				src.TotalAmount,
				src.Description
			));

		CreateMap<ReceiptVM, UpdateReceiptCommand>()
			.ConstructUsing(src => new UpdateReceiptCommand(
				src.Id,
				src.Location,
				src.Date,
				src.TaxAmount,
				src.TotalAmount,
				src.Description
			));
	}

	private void MapTransaction()
	{
		CreateMap<Transaction, TransactionVM>()
			.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount));

		CreateMap<TransactionVM, Transaction>()
			.ConstructUsing(src => Transaction.Create(
				src.ReceiptId,
				src.AccountId,
				new Money(src.Amount, "USD"),
				src.Date
			));
	}

	private void MapReceiptItem()
	{
		CreateMap<ReceiptItem, ReceiptItemVM>()
			.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<ReceiptItemVM, ReceiptItem>()
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