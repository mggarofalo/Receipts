using Application.Commands.Receipt;
using AutoMapper;
using Domain.Core;

namespace Application.Mapping;

public class ReceiptMappingProfile : Profile
{
	public ReceiptMappingProfile()
	{
		CreateMap<CreateReceiptCommand, Receipt>();
		CreateMap<UpdateReceiptCommand, Receipt>();
	}
}