using Application.Commands.Account;
using AutoMapper;
using Domain.Core;

namespace Application.Mapping;

public class AccountMappingProfile : Profile
{
	public AccountMappingProfile()
	{
		CreateMap<CreateAccountCommand, Account>();
		CreateMap<UpdateAccountCommand, Account>();
	}
}