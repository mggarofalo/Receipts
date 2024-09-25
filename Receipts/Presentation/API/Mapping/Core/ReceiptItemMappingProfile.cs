using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

public class ReceiptItemMappingProfile : Profile
{
	public ReceiptItemMappingProfile()
	{
		CreateMap<ReceiptItem, ReceiptItemVM>()
			.ForMember(vm => vm.UnitPrice, opt => opt.MapFrom(domain => domain.UnitPrice.Amount))
			.ForMember(vm => vm.TotalAmount, opt => opt.MapFrom(domain => domain.TotalAmount.Amount));

		CreateMap<ReceiptItemVM, ReceiptItem>()
			.ForMember(domain => domain.UnitPrice, opt => opt.MapFrom(vm => new Money(vm.UnitPrice ?? 0, Currency.USD)))
			.ForMember(domain => domain.TotalAmount, opt => opt.MapFrom(vm => new Money(vm.TotalAmount ?? 0, Currency.USD)));
	}
}