using AutoMapper;
using Domain;
using Domain.Core;
using Application.Commands.Receipt;
using Shared.ViewModels;

namespace API.Mapping;

public class ReceiptMappingProfile : Profile
{
	public ReceiptMappingProfile()
	{
		CreateMap<Receipt, ReceiptVM>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount));

		CreateMap<ReceiptVM, Receipt>()
			.ConstructUsing(src => new(
				null,
				src.Location,
				src.Date,
				new Money(src.TaxAmount, "USD"),
				src.Description
			));

		CreateMap<ReceiptVM, CreateReceiptCommand>()
			.ConstructUsing(src => new CreateReceiptCommand(
				src.Location,
				src.Date,
				src.TaxAmount,
				src.Description
			));

		CreateMap<ReceiptVM, UpdateReceiptCommand>()
			.ConstructUsing(src => new UpdateReceiptCommand(
				src.Id!.Value,
				src.Location,
				src.Date,
				src.TaxAmount,
				src.Description
			));
	}
}