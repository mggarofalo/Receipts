using API.Generated.Dtos;
using Domain.Core;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class AccountMapper
{
	[MapperIgnoreTarget(nameof(AccountResponse.AdditionalProperties))]
	public partial AccountResponse ToResponse(Account source);

	public Account ToDomain(CreateAccountRequest source)
	{
		return new Account(Guid.Empty, source.AccountCode, source.Name, source.IsActive);
	}

	public Account ToDomain(UpdateAccountRequest source)
	{
		return new Account(source.Id, source.AccountCode, source.Name, source.IsActive);
	}
}
